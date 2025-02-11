#include "electionguard/elgamal.hpp"

#include "../../libs/hacl/Lib.hpp"
#include "electionguard/discrete_log.hpp"
#include "electionguard/hash.hpp"
#include "electionguard/hmac.hpp"
#include "electionguard/precompute_buffers.hpp"
#include "facades/bignum4096.hpp"
#include "krml/lowstar_endianness.h"
#include "log.hpp"

#include <array>
#include <electionguard/hash.hpp>
#include <memory>
#include <stdexcept>

using electionguard::HMAC;
using electionguard::facades::Bignum4096;
using std::invalid_argument;
using std::make_unique;
using std::move;
using std::reference_wrapper;
using std::runtime_error;
using std::unique_ptr;

namespace electionguard
{
#pragma region ElgamalKeyPair

    struct ElGamalKeyPair::Impl {
        Impl(unique_ptr<ElementModQ> secretKey, unique_ptr<ElementModP> publicKey)
            : secretKey(move(secretKey)), publicKey(move(publicKey))
        {
        }

        unique_ptr<ElementModQ> secretKey;
        unique_ptr<ElementModP> publicKey;
    };

    // Lifecycle Methods

    ElGamalKeyPair::ElGamalKeyPair(const ElGamalKeyPair &other)
        : pimpl(new Impl(move(*other.pimpl)))
    {
    }

    ElGamalKeyPair::ElGamalKeyPair(unique_ptr<ElementModQ> secretKey,
                                   unique_ptr<ElementModP> publicKey)
        : pimpl(new Impl(move(secretKey), move(publicKey)))
    {
    }

    ElGamalKeyPair &ElGamalKeyPair::operator=(ElGamalKeyPair rhs)
    {
        swap(pimpl, rhs.pimpl);
        return *this;
    }

    ElGamalKeyPair::~ElGamalKeyPair() = default;

    // Property Getters

    ElementModQ *ElGamalKeyPair::getSecretKey() { return pimpl->secretKey.get(); }

    ElementModP *ElGamalKeyPair::getPublicKey() { return pimpl->publicKey.get(); }

    // Public Members

    unique_ptr<ElGamalKeyPair> ElGamalKeyPair::fromSecret(const ElementModQ &secretKey,
                                                          bool isFixedBase /* = true */)
    {
        if (const_cast<ElementModQ &>(secretKey) < TWO_MOD_Q()) {
            throw invalid_argument("ElGamalKeyPair fromSecret secret key needs to be in [2,Q).");
        }
        auto privateKey = make_unique<ElementModQ>(secretKey);
        auto publicKey = g_pow_p(secretKey);
        publicKey->setIsFixedBase(isFixedBase);
        return make_unique<ElGamalKeyPair>(move(privateKey), move(publicKey));
    }

    // TODO: do we really need this??
    unique_ptr<ElGamalKeyPair> ElGamalKeyPair::fromPair(const ElementModQ &secretKey,
                                                        const ElementModP &publicKeyData)
    {
        if (const_cast<ElementModQ &>(secretKey) < TWO_MOD_Q()) {
            throw invalid_argument("ElGamalKeyPair fromSecret secret key needs to be in [2,Q).");
        }
        auto privateKey = make_unique<ElementModQ>(secretKey);
        auto publicKey = make_unique<ElementModP>(publicKeyData);
        return make_unique<ElGamalKeyPair>(move(privateKey), move(publicKey));
    }

#pragma endregion

#pragma region ElGamalCiphertext

    struct ElGamalCiphertext::Impl {
        unique_ptr<ElementModP> pad;
        unique_ptr<ElementModP> data;

        Impl(unique_ptr<ElementModP> pad, unique_ptr<ElementModP> data)
            : pad(move(pad)), data(move(data))
        {
        }

        [[nodiscard]] unique_ptr<ElGamalCiphertext::Impl> clone() const
        {
            auto _pad = make_unique<ElementModP>(*pad);
            auto _data = make_unique<ElementModP>(*data);
            return make_unique<ElGamalCiphertext::Impl>(move(_pad), move(_data));
        }

        [[nodiscard]] unique_ptr<ElementModQ> crypto_hash() const
        {
            return hash_elems({pad.get(), data.get()});
        }

        bool operator==(const Impl &other) { return *pad == *other.pad && *data == *other.data; }
    };

    // Lifecycle Methods

    ElGamalCiphertext::ElGamalCiphertext(const ElGamalCiphertext &other)
        : pimpl(other.pimpl->clone())
    {
    }

    ElGamalCiphertext::ElGamalCiphertext(unique_ptr<ElementModP> pad, unique_ptr<ElementModP> data)
        : pimpl(new Impl(move(pad), move(data)))
    {
    }

    ElGamalCiphertext::~ElGamalCiphertext() = default;

    // Operator Overloads

    ElGamalCiphertext &ElGamalCiphertext::operator=(ElGamalCiphertext rhs)
    {
        swap(pimpl, rhs.pimpl);
        return *this;
    }

    bool ElGamalCiphertext::operator==(const ElGamalCiphertext &other)
    {
        return *pimpl == *other.pimpl;
    }

    bool ElGamalCiphertext::operator!=(const ElGamalCiphertext &other) { return !(*this == other); }

    // Property Getters

    ElementModP *ElGamalCiphertext::getPad() { return pimpl->pad.get(); }
    ElementModP *ElGamalCiphertext::getPad() const { return pimpl->pad.get(); }
    ElementModP *ElGamalCiphertext::getData() { return pimpl->data.get(); }
    ElementModP *ElGamalCiphertext::getData() const { return pimpl->data.get(); }

    // Interface Overrides

    unique_ptr<ElementModQ> ElGamalCiphertext::crypto_hash() { return pimpl->crypto_hash(); }
    unique_ptr<ElementModQ> ElGamalCiphertext::crypto_hash() const { return pimpl->crypto_hash(); }

    unique_ptr<ElGamalCiphertext> ElGamalCiphertext::make(const ElementModP &pad,
                                                          const ElementModP &data)
    {
        return make_unique<ElGamalCiphertext>(make_unique<ElementModP>(pad),
                                              make_unique<ElementModP>(data));
    }

    // Public Methods

    std::unique_ptr<ElGamalCiphertext> ElGamalCiphertext::elgamalAdd(const ElGamalCiphertext &b)
    {
        auto pad = mul_mod_p(*pimpl->pad, *b.pimpl->pad);
        auto data = mul_mod_p(*pimpl->data, *b.pimpl->data);
        return make_unique<ElGamalCiphertext>(move(pad), move(data));
    }

    std::unique_ptr<ElGamalCiphertext> ElGamalCiphertext::elgamalAdd(
      const std::vector<std::reference_wrapper<ElGamalCiphertext>> &ciphertexts)
    {
        auto resultPad = make_unique<ElementModP>(*pimpl->pad);
        auto resultData = make_unique<ElementModP>(*pimpl->data);
        for (auto &ciphertext : ciphertexts) {
            auto pad = mul_mod_p(*resultPad, *ciphertext.get().pimpl->pad);
            resultPad.swap(pad);
            auto data = mul_mod_p(*resultData, *ciphertext.get().pimpl->data);
            resultData.swap(data);
        }
        return make_unique<ElGamalCiphertext>(move(resultPad), move(resultData));
    }

    uint64_t ElGamalCiphertext::decrypt(const ElementModP &product)
    {
        auto result = div_mod_p(*pimpl->data, product);
        return DiscreteLog::getAsync(*result);
    }

    uint64_t ElGamalCiphertext::decrypt(const ElementModP &product) const
    {
        auto result = div_mod_p(*pimpl->data, product);
        return DiscreteLog::getAsync(*result);
    }

    uint64_t ElGamalCiphertext::decrypt(const ElementModQ &secretKey)
    {
        auto product = pow_mod_p(*pimpl->pad, secretKey);
        return decrypt(*product);
    }

    uint64_t ElGamalCiphertext::decrypt(const ElementModQ &secretKey) const
    {
        auto product = pow_mod_p(*pimpl->pad, secretKey);
        return decrypt(*product);
    }

    uint64_t ElGamalCiphertext::decrypt(const ElementModP &publicKey, const ElementModQ &nonce)
    {
        auto product = pow_mod_p(publicKey, nonce);
        return decrypt(*product);
    }

    uint64_t ElGamalCiphertext::decrypt(const ElementModP &publicKey,
                                        const ElementModQ &nonce) const
    {
        auto product = pow_mod_p(publicKey, nonce);
        return decrypt(*product);
    }

    unique_ptr<ElementModP> ElGamalCiphertext::partialDecrypt(const ElementModQ &secretKey)
    {
        return pow_mod_p(*pimpl->pad, secretKey);
    }

    unique_ptr<ElementModP> ElGamalCiphertext::partialDecrypt(const ElementModQ &secretKey) const
    {
        return pow_mod_p(*pimpl->pad, secretKey);
    }

    unique_ptr<ElGamalCiphertext> ElGamalCiphertext::clone() const
    {
        return make_unique<ElGamalCiphertext>(pimpl->pad->clone(), pimpl->data->clone());
    }

#pragma endregion

    unique_ptr<ElGamalCiphertext> elgamalEncrypt(uint64_t m, const ElementModQ &nonce,
                                                 const ElementModP &publicKey)
    {
        if ((const_cast<ElementModQ &>(nonce) == ZERO_MOD_Q())) {
            throw invalid_argument("elgamalEncrypt encryption requires a non-zero nonce");
        }

        auto pad = g_pow_p(nonce);
        auto pubkey_pow_n = pow_mod_p(publicKey, nonce);
        unique_ptr<ElementModP> data = nullptr;
        if (m == 1) {
            data = mul_mod_p(G(), *pubkey_pow_n);
        } else {
            data = move(pubkey_pow_n);
        }

        Log::trace("Generated Encryption");
        Log::trace("publicKey", publicKey.toHex());
        Log::trace("pad", pad->toHex());
        Log::trace("data", data->toHex());

        return make_unique<ElGamalCiphertext>(move(pad), move(data));
    }

    unique_ptr<ElGamalCiphertext> elgamalEncrypt_with_precomputed(uint64_t m, ElementModP &g_to_rho,
                                                                  ElementModP &pubkey_to_rho)
    {
        ElementModP data = pubkey_to_rho;

        if (m == 1) {
            unique_ptr<ElementModP> temp = mul_mod_p(G(), pubkey_to_rho);
            data = *temp;
        }

        Log::trace("Generated Encryption with Precomputed Values");
        Log::trace("pad", g_to_rho.toHex());
        Log::trace("data", data.toHex());

        return make_unique<ElGamalCiphertext>(make_unique<ElementModP>(g_to_rho),
                                              make_unique<ElementModP>(data));
    }

    unique_ptr<ElGamalCiphertext>
    elgamalAdd(const vector<reference_wrapper<ElGamalCiphertext>> &ciphertexts)
    {
        if (ciphertexts.empty()) {
            throw invalid_argument("must have one or more ciphertexts");
        }

        auto resultPad = ElementModP::fromUint64(1UL);
        auto resultData = ElementModP::fromUint64(1UL);
        for (auto ciphertext : ciphertexts) {
            auto pad = mul_mod_p(*resultPad, *ciphertext.get().getPad());
            resultPad.swap(pad);
            auto data = mul_mod_p(*resultData, *ciphertext.get().getData());
            resultData.swap(data);
        }
        return make_unique<ElGamalCiphertext>(move(resultPad), move(resultData));
    }

    unique_ptr<ElGamalCiphertext> elgamalAdd(const ElGamalCiphertext &a, const ElGamalCiphertext &b)
    {
        auto pad = mul_mod_p(*a.getPad(), *b.getPad());
        auto data = mul_mod_p(*a.getData(), *b.getData());
        return make_unique<ElGamalCiphertext>(move(pad), move(data));
    }

#pragma region HashedElGamalCiphertext

    struct HashedElGamalCiphertext::Impl {
        unique_ptr<ElementModP> pad;
        vector<uint8_t> data;
        vector<uint8_t> mac;

        Impl(unique_ptr<ElementModP> pad, vector<uint8_t> data, vector<uint8_t> mac)
            : pad(move(pad)), data(data), mac(mac)
        {
        }

        [[nodiscard]] unique_ptr<HashedElGamalCiphertext::Impl> clone() const
        {
            auto _pad = make_unique<ElementModP>(*pad);
            return make_unique<HashedElGamalCiphertext::Impl>(move(_pad), data, mac);
        }

        [[nodiscard]] unique_ptr<ElementModQ> crypto_hash() const
        {
            return hash_elems({pad.get(), data, mac});
        }

        bool operator==(const Impl &other)
        {
            return *pad == *other.pad && data == other.data && mac == other.mac;
        }
    };

    // Lifecycle Methods

    HashedElGamalCiphertext::HashedElGamalCiphertext(const HashedElGamalCiphertext &other)
        : pimpl(other.pimpl->clone())
    {
    }

    HashedElGamalCiphertext::HashedElGamalCiphertext(std::unique_ptr<ElementModP> pad,
                                                     std::vector<uint8_t> data,
                                                     std::vector<uint8_t> mac)
        : pimpl(new Impl(move(pad), data, mac))
    {
    }

    HashedElGamalCiphertext::~HashedElGamalCiphertext() = default;

    // Operator Overloads

    HashedElGamalCiphertext &HashedElGamalCiphertext::operator=(HashedElGamalCiphertext rhs)
    {
        swap(pimpl, rhs.pimpl);
        return *this;
    }

    bool HashedElGamalCiphertext::operator==(const HashedElGamalCiphertext &other)
    {
        return *pimpl == *other.pimpl;
    }

    bool HashedElGamalCiphertext::operator!=(const HashedElGamalCiphertext &other)
    {
        return !(*this == other);
    }

    // Property Getters

    ElementModP *HashedElGamalCiphertext::getPad() { return pimpl->pad.get(); }
    ElementModP *HashedElGamalCiphertext::getPad() const { return pimpl->pad.get(); }
    vector<uint8_t> HashedElGamalCiphertext::getData() { return pimpl->data; }
    vector<uint8_t> HashedElGamalCiphertext::getData() const { return pimpl->data; }
    vector<uint8_t> HashedElGamalCiphertext::getMac() { return pimpl->mac; }
    vector<uint8_t> HashedElGamalCiphertext::getMac() const { return pimpl->mac; }

    unique_ptr<ElementModQ> HashedElGamalCiphertext::crypto_hash() { return pimpl->crypto_hash(); }
    unique_ptr<ElementModQ> HashedElGamalCiphertext::crypto_hash() const
    {
        return pimpl->crypto_hash();
    }

    // Public Methods

    unique_ptr<HashedElGamalCiphertext> HashedElGamalCiphertext::make(const ElementModP &pad,
                                                                      std::vector<uint8_t> data,
                                                                      std::vector<uint8_t> mac)
    {
        return make_unique<HashedElGamalCiphertext>(make_unique<ElementModP>(pad), data, mac);
    }

    vector<uint8_t> HashedElGamalCiphertext::decrypt(const ElementModQ &secret_key,
                                                     const ElementModQ &encryption_seed,
                                                     bool look_for_padding)
    {
        // Note this decryption method is primarily used for testing
        vector<uint8_t> plaintext_with_padding;
        vector<uint8_t> plaintext;

        uint32_t ciphertext_len = pimpl->data.size();
        uint32_t number_of_blocks = ciphertext_len / HASHED_CIPHERTEXT_BLOCK_LENGTH;
        if ((0 != (ciphertext_len % HASHED_CIPHERTEXT_BLOCK_LENGTH)) || (ciphertext_len == 0)) {
            throw invalid_argument("HashedElGamalCiphertext::decrypt the ciphertext "
                                   "is not a multiple of the block length 32");
        }

        auto publicKey_to_r = pow_mod_p(*pimpl->pad, secret_key);

        // hash g_to_r and publicKey_to_r to get the session key
        auto session_key = hash_elems({pimpl->pad.get(), publicKey_to_r.get()});

        vector<uint8_t> mac_key = HMAC::compute(session_key->toBytes(), encryption_seed.toBytes(),
                                                number_of_blocks * HASHED_BLOCK_LENGTH_IN_BITS, 0);

        // calculate the mac (c0 is g ^ r mod p and c1 is the ciphertext, they are concatenated)
        vector<uint8_t> c0_and_c1(pimpl->pad->toBytes());
        c0_and_c1.insert(c0_and_c1.end(), pimpl->data.begin(), pimpl->data.end());
        vector<uint8_t> our_mac = HMAC::compute(mac_key, c0_and_c1, 0, 0);
        hacl::Lib::memZero(&mac_key.front(), mac_key.size());

        if (pimpl->mac != our_mac) {
            throw runtime_error(
              "HashedElGamalCiphertext::decrypt the calculated mac didn't match the passed in mac");
        }

        uint32_t plaintext_index = 0;
        for (uint32_t i = 0; i < number_of_blocks; i++) {
            vector<int8_t> temp_plaintext(HASHED_CIPHERTEXT_BLOCK_LENGTH, 0);

            vector<uint8_t> xor_key =
              HMAC::compute(session_key->toBytes(), encryption_seed.toBytes(),
                            number_of_blocks * HASHED_BLOCK_LENGTH_IN_BITS, i + 1);

            // XOR the key with the plaintext
            for (int j = 0; j < (int)HASHED_CIPHERTEXT_BLOCK_LENGTH; j++) {
                temp_plaintext[j] = pimpl->data[plaintext_index] ^ xor_key[j];
                // advance the plaintext index
                plaintext_index++;
            }
            hacl::Lib::memZero(&xor_key.front(), xor_key.size());

            plaintext_with_padding.insert(plaintext_with_padding.end(), temp_plaintext.begin(),
                                          temp_plaintext.end());
            hacl::Lib::memZero(&temp_plaintext.front(), temp_plaintext.size());
        }

        if (look_for_padding) {
            uint16_t pad_len_be;
            memcpy(&pad_len_be, &plaintext_with_padding.front(), sizeof(pad_len_be));
            uint16_t pad_len = be16toh(pad_len_be);

            if (pad_len > (plaintext_with_padding.size() - sizeof(pad_len))) {
                throw runtime_error(
                  "HashedElGamalCiphertext::decrypt the padding is incorrect, decrypt failed");
            }

            // check that the end bytes are 0x00
            if (pad_len > 0) {
                for (int i = 1; i <= (int)pad_len; i++) {
                    if (plaintext_with_padding[plaintext_with_padding.size() - i] != 0x00) {
                        throw runtime_error("HashedElGamalCiphertext::decrypt the padding is "
                                            "incorrect, decrypt failed");
                    }
                }
            }

            plaintext.insert(plaintext.end(), &plaintext_with_padding.front() + sizeof(pad_len),
                             &plaintext_with_padding.front() +
                               (plaintext_with_padding.size() - pad_len));
        } else {
            plaintext = plaintext_with_padding;
        }

        return plaintext;
    }

    /// <Summary>
    /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
    /// 𝑀_i = C0^P𝑖 mod 𝑝 in the spec
    ///
    /// <param name="secretKey">The corresponding ElGamal secret key.</param>
    /// <returns>A partial decryption of the plaintext value</returns
    /// </Summary>
    unique_ptr<ElementModP> HashedElGamalCiphertext::partialDecrypt(const ElementModQ &secretKey)
    {
        return pow_mod_p(*pimpl->pad, secretKey);
    }

    /// <Summary>
    /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
    /// 𝑀_i = C0^P𝑖 mod 𝑝 in the spec
    ///
    /// <param name="secretKey">The corresponding ElGamal secret key.</param>
    /// <returns>A partial decryption of the plaintext value</returns
    /// </Summary>
    unique_ptr<ElementModP>
    HashedElGamalCiphertext::partialDecrypt(const ElementModQ &secretKey) const
    {
        return pow_mod_p(*pimpl->pad, secretKey);
    }

    unique_ptr<HashedElGamalCiphertext> HashedElGamalCiphertext::clone() const
    {
        return make_unique<HashedElGamalCiphertext>(pimpl->pad->clone(), pimpl->data, pimpl->mac);
    }

#pragma endregion // HashedElGamalCiphertext

    unique_ptr<HashedElGamalCiphertext>
    hashedElgamalEncrypt(std::vector<uint8_t> message, const ElementModQ &nonce,
                         const ElementModP &publicKey, const ElementModQ &encryption_seed,
                         padded_data_size_t max_len, bool allow_truncation,
                         bool shouldUsePrecomputedValues /* = false */)
    {
        vector<uint8_t> ciphertext;
        vector<uint8_t> plaintext_on_boundary;

        // padding scheme is to concatenate [length of the padding][plaintext][padding bytes of 0x00]
        // padding bytes 0x00 are padded out to the first HASHED_CIPHERTEXT_BLOCK_LENGTH boundary
        // past max_len. So if max_len is 62 then it will pad to the 64 byte boundary
        if (max_len != NO_PADDING) {
            uint16_t pad_len = 0;
            uint16_t pad_len_be = 0;

            if (allow_truncation && (message.size() > max_len)) {
                // truncate the data
                // insert length in big endian form
                plaintext_on_boundary.insert(plaintext_on_boundary.end(), (uint8_t *)&pad_len_be,
                                             (uint8_t *)&pad_len_be + sizeof(pad_len_be));
                // insert plaintext
                plaintext_on_boundary.insert(plaintext_on_boundary.end(), &message.front(),
                                             &message.front() + max_len);
            } else {
                if (message.size() > max_len) {
                    throw invalid_argument(
                      "HashedElGamalCiphertext::encrypt the plaintext is greater than max_len");
                }

                uint16_t pad_len = max_len - message.size();
                uint16_t pad_len_be = htobe16(pad_len);

                std::vector<uint8_t> padding(pad_len, 0);

                // insert length in big endian form
                plaintext_on_boundary.insert(plaintext_on_boundary.end(), (uint8_t *)&pad_len_be,
                                             (uint8_t *)&pad_len_be + sizeof(pad_len_be));
                // insert plaintext
                plaintext_on_boundary.insert(plaintext_on_boundary.end(), message.begin(),
                                             message.end());

                // we dont pad 0x00s if the length field plus plaintext length falls on a block length boundary
                if (pad_len > 0) {
                    // insert padding
                    plaintext_on_boundary.insert(plaintext_on_boundary.end(), padding.begin(),
                                                 padding.end());
                }
            }
        } else {
            if (0 != (message.size() % HASHED_CIPHERTEXT_BLOCK_LENGTH)) {
                throw invalid_argument(
                  "HashedElGamalCiphertext::encrypt the apply_padding was false "
                  "but the plaintext is not a multiple of the block length 32");
            }
            plaintext_on_boundary.insert(plaintext_on_boundary.end(), message.begin(),
                                         message.end());
        }

        uint32_t plaintext_len = plaintext_on_boundary.size();
        uint32_t number_of_blocks = plaintext_len / HASHED_CIPHERTEXT_BLOCK_LENGTH;

        unique_ptr<ElementModP> g_to_r = nullptr;
        unique_ptr<ElementModP> publicKey_to_r = nullptr;

        if (shouldUsePrecomputedValues) {
            // check if the are precompute values rather than doing the exponentiations here
            auto triple = PrecomputeBufferContext::popTriple();
            if (triple != nullptr && triple.has_value()) {
                g_to_r = triple.value()->get_g_to_exp();
                publicKey_to_r = triple.value()->get_pubkey_to_exp();
            }
        }

        // fallback to doing the exponentiations here
        if (g_to_r == nullptr || publicKey_to_r == nullptr) {
            g_to_r = g_pow_p(nonce);
            publicKey_to_r = pow_mod_p(publicKey, nonce);
        }

        // hash g_to_r and publicKey_to_r to get the session key
        auto session_key = hash_elems({g_to_r.get(), publicKey_to_r.get()});

        uint32_t plaintext_index = 0;
        for (uint32_t i = 0; i < number_of_blocks; i++) {
            vector<uint8_t> temp_ciphertext(HASHED_CIPHERTEXT_BLOCK_LENGTH, 0);

            vector<uint8_t> xor_key =
              HMAC::compute(session_key->toBytes(), encryption_seed.toBytes(),
                            number_of_blocks * HASHED_BLOCK_LENGTH_IN_BITS, i + 1);

            // XOR the key with the plaintext
            for (int j = 0; j < (int)HASHED_CIPHERTEXT_BLOCK_LENGTH; j++) {
                temp_ciphertext[j] = plaintext_on_boundary[plaintext_index] ^ xor_key[j];
                // advance the plaintext index
                plaintext_index++;
            }
            hacl::Lib::memZero(&xor_key.front(), xor_key.size());

            ciphertext.insert(ciphertext.end(), temp_ciphertext.begin(), temp_ciphertext.end());
            hacl::Lib::memZero(&temp_ciphertext.front(), temp_ciphertext.size());
        }

        vector<uint8_t> mac_key = HMAC::compute(session_key->toBytes(), encryption_seed.toBytes(),
                                                number_of_blocks * HASHED_BLOCK_LENGTH_IN_BITS, 0);

        // calculate the mac (c0 is g ^ r mod p and c1 is the ciphertext, they are concatenated)
        vector<uint8_t> c0_and_c1(g_to_r->toBytes());
        c0_and_c1.insert(c0_and_c1.end(), ciphertext.begin(), ciphertext.end());
        vector<uint8_t> mac = HMAC::compute(mac_key, c0_and_c1, 0, 0);
        hacl::Lib::memZero(&mac_key.front(), mac_key.size());

        return make_unique<HashedElGamalCiphertext>(move(g_to_r), ciphertext, mac);
    }

} // namespace electionguard

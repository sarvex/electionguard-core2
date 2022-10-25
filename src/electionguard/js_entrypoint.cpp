#include "js_entrypoint.hpp"
#include "electionguard/election.hpp"
#include "electionguard/manifest.hpp"
#include "variant_cast.hpp"
#include "electionguard/election.h"
#include "electionguard/encrypt.hpp"
#include "electionguard/encrypt.h"

using electionguard::CiphertextElectionContext;
using electionguard::Manifest;
using electionguard::EncryptionDevice;

// todo: try char * to remove warning?
const char *js_encrypt(const char *context, const char *manifest, const char *ballot)
{
    // get context
    auto context_obj = CiphertextElectionContext::fromJson(std::string(context));
    // get manifest 
    auto manifest_obj = Manifest::fromJson(std::string(manifest));
    // make ballot - eg_encryption_device_new
    auto device = make_unique<EncryptionDevice>(12345UL, 23456UL, 34567UL, "Location");
    // make encryption mediator - eg_encryption_mediator_new
    // make a plaintext ballot - eg_plaintext_ballot_from_json
    // encrypt - eg_encryption_mediator_encrypt_ballot
    // convert to json - eg_ciphertext_ballot_to_json

    delete AS_TYPE(eg_ciphertext_election_context_t, manifest_obj.release());
    delete AS_TYPE(eg_context_configuration_t, context_obj.release());
    delete AS_TYPE(eg_encryption_device_t, device.release());

    std::string intermediate = "hello world";
    auto result = intermediate.c_str();
    // std::string intermediate = std::string(ballot);
    return result;
}
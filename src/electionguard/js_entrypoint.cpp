#include "js_entrypoint.hpp"
#include "electionguard/election.hpp"

// todo: try char * to remove warning?
const char *js_encrypt(const char *context, const char *manifest, const char *ballot)
{
    // get context
    auto context_obj = electionguard::CiphertextElectionContext::fromJson(std::string(context));
    // get manifest - eg_election_manifest_from_json
    // make ballot - eg_encryption_device_new
    // make encryption mediator - eg_encryption_mediator_new
    // make a plaintext ballot - eg_plaintext_ballot_from_json
    // encrypt - eg_encryption_mediator_encrypt_ballot
    // convert to json - eg_ciphertext_ballot_to_json

    std::string intermediate = std::string(manifest);
    auto result = intermediate.c_str();
    return result;
}
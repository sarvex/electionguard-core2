#include "js_entrypoint.hpp"
#include "electionguard/election.hpp"
#include "electionguard/manifest.hpp"
#include "variant_cast.hpp"
#include "electionguard/election.h"
#include "electionguard/encrypt.hpp"
#include "electionguard/encrypt.h"
#include "electionguard/ballot.hpp"
#include "electionguard/ballot.h"

#include <iostream>
#include <chrono>
#include <ctime>

using std::string;
using electionguard::CiphertextElectionContext;
using electionguard::Manifest;
using electionguard::InternalManifest;
using electionguard::EncryptionDevice;
using electionguard::EncryptionMediator;
using electionguard::PlaintextBallot;

const char *js_encrypt(const char *context, const char *manifest, const char *ballot)
{
    // duration calculation
    auto start = std::chrono::system_clock::now();
    std::time_t start_time = std::chrono::system_clock::to_time_t(start);
    std::cout << "started encryption at " << std::ctime(&start_time) << std::endl;
    // get context
    auto context_obj = CiphertextElectionContext::fromJson(string(context));
    // get manifest 
    auto manifest_obj = Manifest::fromJson(string(manifest));
    auto internal = make_unique<InternalManifest>(*manifest_obj);
    // make device
    auto device = make_unique<EncryptionDevice>(12345UL, 23456UL, 34567UL, "Location");
    // make encryption mediator
    auto mediator = make_unique<EncryptionMediator>(*internal, *context_obj, *device);
    // make ballot
    auto ballot_obj = PlaintextBallot::fromJson(string(ballot));
    // encrypt
    auto ciphertext = mediator->encrypt(*ballot_obj);
    auto end = std::chrono::system_clock::now();
    std::chrono::duration<double> elapsed_seconds = end-start;
    std::time_t end_time = std::chrono::system_clock::to_time_t(end);
    std::cout << "finished encryption at " << std::ctime(&end_time)
              << "elapsed time: " << elapsed_seconds.count() << "s"
              << std::endl;
    // convert to json
    std::cout << "encryption successful" << std::endl;
    auto json = ciphertext->toJson(false);
    std::cout << "toJson successful" << std::endl;

    delete AS_TYPE(eg_ciphertext_election_context_t, context_obj.release());
    // or maybe: delete AS_TYPE(eg_context_configuration_t, context_obj.release());

    delete AS_TYPE(eg_election_manifest_t, manifest_obj.release());
    delete AS_TYPE(eg_encryption_device_t, device.release());
    delete AS_TYPE(eg_encryption_mediator_t, mediator.release());
    delete AS_TYPE(eg_plaintext_ballot_t, ballot_obj.release());
    delete AS_TYPE(eg_internal_manifest_t, internal.release());

    auto result = json.c_str();
    return result;
}
#ifndef __ELECTIONGUARD_CPP_TOOLS_GENERATORS_ELECTION_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_TOOLS_GENERATORS_ELECTION_HPP_INCLUDED__

#include <electionguard/election.hpp>
#include <electionguard/export.h>
#include <electionguard/manifest.hpp>
#include <fstream>
#include <iostream>
#include <sstream>

using namespace electionguard;
using namespace std;

namespace electionguard::tools::generators
{
    class ElectionGenerator
    {
      public:
        static unique_ptr<CiphertextElectionContext>
        getFakeContext(const InternalManifest &manifest, const ElementModP &publicKey)
        {
            auto context =
              CiphertextElectionContext::make(1UL, 1UL, publicKey.clone(), TWO_MOD_Q().clone(),
                                              manifest.getManifestHash()->clone());
            return context;
        }
    };
} // namespace electionguard::tools::generators

#endif /* __ELECTIONGUARD_CPP_TOOLS_GENERATORS_ELECTION_HPP_INCLUDED__ */

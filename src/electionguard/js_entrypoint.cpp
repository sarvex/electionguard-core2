#include "js_entrypoint.hpp"
#include "facades/Hacl_Bignum256.hpp"

uint64_t cpp_to_wasm_add(uint64_t a, uint64_t b)
{
	uint64_t result = 0;
	hacl::Bignum256::add(&a, &b, &result);
	return result;
}

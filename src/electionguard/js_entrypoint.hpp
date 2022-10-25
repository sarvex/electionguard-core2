#pragma once
#include <cstdint>
#include <string>

extern "C" {
	const char *js_encrypt(const char *context, const char *manifest, const char *ballot);
}

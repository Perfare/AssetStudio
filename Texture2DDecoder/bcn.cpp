#include "bcn.h"
#include <stdint.h>
#include <assert.h>
#include <algorithm>
#include "color.h"
#include "fp16.h"

static inline void decode_bc1_block(const uint8_t* data, uint32_t* outbuf) {
	uint8_t r0, g0, b0, r1, g1, b1;
	int q0 = *(uint16_t*)(data);
	int q1 = *(uint16_t*)(data + 2);
	rgb565_le(q0, &r0, &g0, &b0);
	rgb565_le(q1, &r1, &g1, &b1);
	uint_fast32_t c[4] = { color(r0, g0, b0, 255), color(r1, g1, b1, 255) };
	if (q0 > q1) {
		c[2] = color((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3, 255);
		c[3] = color((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3, 255);
	}
	else {
		c[2] = color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 255);
		c[3] = color(0, 0, 0, 255);
	}
	uint_fast32_t d = lton32(*(uint32_t*)(data + 4));
	for (int i = 0; i < 16; i++, d >>= 2)
		outbuf[i] = c[d & 3];
}

int decode_bc1(const uint8_t* data, const long w, const long h, uint32_t* image) {
	long num_blocks_x = (w + 3) / 4;
	long num_blocks_y = (h + 3) / 4;
	uint32_t buffer[16];
	const uint8_t* d = data;
	for (long by = 0; by < num_blocks_y; by++) {
		for (long bx = 0; bx < num_blocks_x; bx++, d += 8) {
			decode_bc1_block(d, buffer);
			copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
		}
	}
	return 1;
}

void decode_bc3_alpha(const uint8_t* data, uint32_t* outbuf, int channel) {
	uint_fast8_t a[8] = { data[0], data[1] };
	if (a[0] > a[1]) {
		a[2] = (a[0] * 6 + a[1]) / 7;
		a[3] = (a[0] * 5 + a[1] * 2) / 7;
		a[4] = (a[0] * 4 + a[1] * 3) / 7;
		a[5] = (a[0] * 3 + a[1] * 4) / 7;
		a[6] = (a[0] * 2 + a[1] * 5) / 7;
		a[7] = (a[0] + a[1] * 6) / 7;
	}
	else {
		a[2] = (a[0] * 4 + a[1]) / 5;
		a[3] = (a[0] * 3 + a[1] * 2) / 5;
		a[4] = (a[0] * 2 + a[1] * 3) / 5;
		a[5] = (a[0] + a[1] * 4) / 5;
		a[6] = 0;
		a[7] = 255;
	}

	uint8_t* dst = (uint8_t*)outbuf;
	uint_fast64_t d = lton64(*(uint64_t*)data) >> 16;
	for (int i = 0; i < 16; i++, d >>= 3)
		dst[i * 4 + channel] = a[d & 7];
}

static inline void decode_bc3_block(const uint8_t* data, uint32_t* outbuf) {
	decode_bc1_block(data + 8, outbuf);
	decode_bc3_alpha(data, outbuf, 3);
}

int decode_bc3(const uint8_t* data, const long w, const long h, uint32_t* image) {
	long num_blocks_x = (w + 3) / 4;
	long num_blocks_y = (h + 3) / 4;
	uint32_t buffer[16];
	const uint8_t* d = data;
	for (long by = 0; by < num_blocks_y; by++) {
		for (long bx = 0; bx < num_blocks_x; bx++, d += 16) {
			decode_bc3_block(d, buffer);
			copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
		}
	}
	return 1;
}

static inline void decode_bc4_block(const uint8_t* data, uint32_t* outbuf) {
	decode_bc3_alpha(data, outbuf, 2);
}

int decode_bc4(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image) {
	uint32_t m_block_width = 4;
	uint32_t m_block_height = 4;
	uint32_t m_blocks_x = (m_width + m_block_width - 1) / m_block_width;
	uint32_t m_blocks_y = (m_height + m_block_height - 1) / m_block_height;
	uint32_t buffer[16];
	for (uint32_t i = 0; i < 16; i++)
		buffer[i] = 0xff000000;
	for (uint32_t by = 0; by < m_blocks_y; by++) {
		for (uint32_t bx = 0; bx < m_blocks_x; bx++, data += 8) {
			decode_bc4_block(data, buffer);
			copy_block_buffer(bx, by, m_width, m_height, m_block_width, m_block_height, buffer, image);
		}
	}
	return 1;
}

static inline void decode_bc5_block(const uint8_t* data, uint32_t* outbuf) {
	decode_bc3_alpha(data, outbuf, 2);
	decode_bc3_alpha(data + 8, outbuf, 1);
}

int decode_bc5(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image) {
	uint32_t m_block_width = 4;
	uint32_t m_block_height = 4;
	uint32_t m_blocks_x = (m_width + m_block_width - 1) / m_block_width;
	uint32_t m_blocks_y = (m_height + m_block_height - 1) / m_block_height;
	uint32_t buffer[16];
	for (uint32_t i = 0; i < 16; i++)
		buffer[i] = 0xff000000;
	for (uint32_t by = 0; by < m_blocks_y; by++) {
		for (uint32_t bx = 0; bx < m_blocks_x; bx++, data += 16) {
			decode_bc5_block(data, buffer);
			copy_block_buffer(bx, by, m_width, m_height, m_block_width, m_block_height, buffer, image);
		}
	}
	return 1;
}

struct BitReader
{
	BitReader(const uint8_t* _data, uint16_t _bitPos = 0)
		: m_data(_data)
		, m_bitPos(_bitPos)
	{
	}

	uint16_t read(uint8_t _numBits)
	{
		const uint16_t pos = m_bitPos / 8;
		const uint16_t shift = m_bitPos & 7;
		uint32_t data = 0;
		memcpy(&data, &m_data[pos], std::min(4, 16 - pos));
		m_bitPos += _numBits;
		return uint16_t((data >> shift) & ((1 << _numBits) - 1));
	}

	uint16_t peek(uint16_t _offset, uint8_t _numBits)
	{
		const uint16_t bitPos = m_bitPos + _offset;
		const uint16_t shift = bitPos & 7;
		uint16_t pos = bitPos / 8;
		uint32_t data = 0;
		memcpy(&data, &m_data[pos], std::min(4, 16 - pos));
		return uint8_t((data >> shift) & ((1 << _numBits) - 1));
	}

	const uint8_t* m_data;
	uint16_t m_bitPos;
};

static const uint16_t s_bptcP2[] =
{ //  3210     0000000000   1111111111   2222222222   3333333333
	0xcccc, // 0, 0, 1, 1,  0, 0, 1, 1,  0, 0, 1, 1,  0, 0, 1, 1
	0x8888, // 0, 0, 0, 1,  0, 0, 0, 1,  0, 0, 0, 1,  0, 0, 0, 1
	0xeeee, // 0, 1, 1, 1,  0, 1, 1, 1,  0, 1, 1, 1,  0, 1, 1, 1
	0xecc8, // 0, 0, 0, 1,  0, 0, 1, 1,  0, 0, 1, 1,  0, 1, 1, 1
	0xc880, // 0, 0, 0, 0,  0, 0, 0, 1,  0, 0, 0, 1,  0, 0, 1, 1
	0xfeec, // 0, 0, 1, 1,  0, 1, 1, 1,  0, 1, 1, 1,  1, 1, 1, 1
	0xfec8, // 0, 0, 0, 1,  0, 0, 1, 1,  0, 1, 1, 1,  1, 1, 1, 1
	0xec80, // 0, 0, 0, 0,  0, 0, 0, 1,  0, 0, 1, 1,  0, 1, 1, 1
	0xc800, // 0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 1,  0, 0, 1, 1
	0xffec, // 0, 0, 1, 1,  0, 1, 1, 1,  1, 1, 1, 1,  1, 1, 1, 1
	0xfe80, // 0, 0, 0, 0,  0, 0, 0, 1,  0, 1, 1, 1,  1, 1, 1, 1
	0xe800, // 0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 1,  0, 1, 1, 1
	0xffe8, // 0, 0, 0, 1,  0, 1, 1, 1,  1, 1, 1, 1,  1, 1, 1, 1
	0xff00, // 0, 0, 0, 0,  0, 0, 0, 0,  1, 1, 1, 1,  1, 1, 1, 1
	0xfff0, // 0, 0, 0, 0,  1, 1, 1, 1,  1, 1, 1, 1,  1, 1, 1, 1
	0xf000, // 0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  1, 1, 1, 1
	0xf710, // 0, 0, 0, 0,  1, 0, 0, 0,  1, 1, 1, 0,  1, 1, 1, 1
	0x008e, // 0, 1, 1, 1,  0, 0, 0, 1,  0, 0, 0, 0,  0, 0, 0, 0
	0x7100, // 0, 0, 0, 0,  0, 0, 0, 0,  1, 0, 0, 0,  1, 1, 1, 0
	0x08ce, // 0, 1, 1, 1,  0, 0, 1, 1,  0, 0, 0, 1,  0, 0, 0, 0
	0x008c, // 0, 0, 1, 1,  0, 0, 0, 1,  0, 0, 0, 0,  0, 0, 0, 0
	0x7310, // 0, 0, 0, 0,  1, 0, 0, 0,  1, 1, 0, 0,  1, 1, 1, 0
	0x3100, // 0, 0, 0, 0,  0, 0, 0, 0,  1, 0, 0, 0,  1, 1, 0, 0
	0x8cce, // 0, 1, 1, 1,  0, 0, 1, 1,  0, 0, 1, 1,  0, 0, 0, 1
	0x088c, // 0, 0, 1, 1,  0, 0, 0, 1,  0, 0, 0, 1,  0, 0, 0, 0
	0x3110, // 0, 0, 0, 0,  1, 0, 0, 0,  1, 0, 0, 0,  1, 1, 0, 0
	0x6666, // 0, 1, 1, 0,  0, 1, 1, 0,  0, 1, 1, 0,  0, 1, 1, 0
	0x366c, // 0, 0, 1, 1,  0, 1, 1, 0,  0, 1, 1, 0,  1, 1, 0, 0
	0x17e8, // 0, 0, 0, 1,  0, 1, 1, 1,  1, 1, 1, 0,  1, 0, 0, 0
	0x0ff0, // 0, 0, 0, 0,  1, 1, 1, 1,  1, 1, 1, 1,  0, 0, 0, 0
	0x718e, // 0, 1, 1, 1,  0, 0, 0, 1,  1, 0, 0, 0,  1, 1, 1, 0
	0x399c, // 0, 0, 1, 1,  1, 0, 0, 1,  1, 0, 0, 1,  1, 1, 0, 0
	0xaaaa, // 0, 1, 0, 1,  0, 1, 0, 1,  0, 1, 0, 1,  0, 1, 0, 1
	0xf0f0, // 0, 0, 0, 0,  1, 1, 1, 1,  0, 0, 0, 0,  1, 1, 1, 1
	0x5a5a, // 0, 1, 0, 1,  1, 0, 1, 0,  0, 1, 0, 1,  1, 0, 1, 0
	0x33cc, // 0, 0, 1, 1,  0, 0, 1, 1,  1, 1, 0, 0,  1, 1, 0, 0
	0x3c3c, // 0, 0, 1, 1,  1, 1, 0, 0,  0, 0, 1, 1,  1, 1, 0, 0
	0x55aa, // 0, 1, 0, 1,  0, 1, 0, 1,  1, 0, 1, 0,  1, 0, 1, 0
	0x9696, // 0, 1, 1, 0,  1, 0, 0, 1,  0, 1, 1, 0,  1, 0, 0, 1
	0xa55a, // 0, 1, 0, 1,  1, 0, 1, 0,  1, 0, 1, 0,  0, 1, 0, 1
	0x73ce, // 0, 1, 1, 1,  0, 0, 1, 1,  1, 1, 0, 0,  1, 1, 1, 0
	0x13c8, // 0, 0, 0, 1,  0, 0, 1, 1,  1, 1, 0, 0,  1, 0, 0, 0
	0x324c, // 0, 0, 1, 1,  0, 0, 1, 0,  0, 1, 0, 0,  1, 1, 0, 0
	0x3bdc, // 0, 0, 1, 1,  1, 0, 1, 1,  1, 1, 0, 1,  1, 1, 0, 0
	0x6996, // 0, 1, 1, 0,  1, 0, 0, 1,  1, 0, 0, 1,  0, 1, 1, 0
	0xc33c, // 0, 0, 1, 1,  1, 1, 0, 0,  1, 1, 0, 0,  0, 0, 1, 1
	0x9966, // 0, 1, 1, 0,  0, 1, 1, 0,  1, 0, 0, 1,  1, 0, 0, 1
	0x0660, // 0, 0, 0, 0,  0, 1, 1, 0,  0, 1, 1, 0,  0, 0, 0, 0
	0x0272, // 0, 1, 0, 0,  1, 1, 1, 0,  0, 1, 0, 0,  0, 0, 0, 0
	0x04e4, // 0, 0, 1, 0,  0, 1, 1, 1,  0, 0, 1, 0,  0, 0, 0, 0
	0x4e40, // 0, 0, 0, 0,  0, 0, 1, 0,  0, 1, 1, 1,  0, 0, 1, 0
	0x2720, // 0, 0, 0, 0,  0, 1, 0, 0,  1, 1, 1, 0,  0, 1, 0, 0
	0xc936, // 0, 1, 1, 0,  1, 1, 0, 0,  1, 0, 0, 1,  0, 0, 1, 1
	0x936c, // 0, 0, 1, 1,  0, 1, 1, 0,  1, 1, 0, 0,  1, 0, 0, 1
	0x39c6, // 0, 1, 1, 0,  0, 0, 1, 1,  1, 0, 0, 1,  1, 1, 0, 0
	0x639c, // 0, 0, 1, 1,  1, 0, 0, 1,  1, 1, 0, 0,  0, 1, 1, 0
	0x9336, // 0, 1, 1, 0,  1, 1, 0, 0,  1, 1, 0, 0,  1, 0, 0, 1
	0x9cc6, // 0, 1, 1, 0,  0, 0, 1, 1,  0, 0, 1, 1,  1, 0, 0, 1
	0x817e, // 0, 1, 1, 1,  1, 1, 1, 0,  1, 0, 0, 0,  0, 0, 0, 1
	0xe718, // 0, 0, 0, 1,  1, 0, 0, 0,  1, 1, 1, 0,  0, 1, 1, 1
	0xccf0, // 0, 0, 0, 0,  1, 1, 1, 1,  0, 0, 1, 1,  0, 0, 1, 1
	0x0fcc, // 0, 0, 1, 1,  0, 0, 1, 1,  1, 1, 1, 1,  0, 0, 0, 0
	0x7744, // 0, 0, 1, 0,  0, 0, 1, 0,  1, 1, 1, 0,  1, 1, 1, 0
	0xee22, // 0, 1, 0, 0,  0, 1, 0, 0,  0, 1, 1, 1,  0, 1, 1, 1
};

static const uint8_t s_bptcA2[] =
{
	15, 15, 15, 15, 15, 15, 15, 15,
	15, 15, 15, 15, 15, 15, 15, 15,
	15,  2,  8,  2,  2,  8,  8, 15,
	 2,  8,  2,  2,  8,  8,  2,  2,
	15, 15,  6,  8,  2,  8, 15, 15,
	 2,  8,  2,  2,  2, 15, 15,  6,
	 6,  2,  6,  8, 15, 15,  2,  2,
	15, 15, 15, 15, 15,  2,  2, 15,
};

static const uint8_t s_bptcFactors[3][16] =
{
	{  0, 21, 43, 64,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
	{  0,  9, 18, 27, 37, 46, 55, 64,  0,  0,  0,  0,  0,  0,  0,  0 },
	{  0,  4,  9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 },
};

struct Bc6hModeInfo
{
	uint8_t transformed;
	uint8_t partitionBits;
	uint8_t endpointBits;
	uint8_t deltaBits[3];
};

static const Bc6hModeInfo s_bc6hModeInfo[] =
{ //  +--------------------------- transformed
  //  |  +------------------------ partition bits
  //  |  |  +--------------------- endpoint bits
  //  |  |  |      +-------------- delta bits
	{ 1, 5, 10, {  5,  5,  5 } }, // 00    2-bits
	{ 1, 5,  7, {  6,  6,  6 } }, // 01
	{ 1, 5, 11, {  5,  4,  4 } }, // 00010 5-bits
	{ 0, 0, 10, { 10, 10, 10 } }, // 00011
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 1, 5, 11, {  4,  5,  4 } }, // 00110
	{ 1, 0, 11, {  9,  9,  9 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 1, 5, 11, {  4,  4,  5 } }, // 00010
	{ 1, 0, 12, {  8,  8,  8 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 1, 5,  9, {  5,  5,  5 } }, // 00010
	{ 1, 0, 16, {  4,  4,  4 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 1, 5,  8, {  6,  5,  5 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 1, 5,  8, {  5,  6,  5 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 1, 5,  8, {  5,  5,  6 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 0,  0, {  0,  0,  0 } }, // -
	{ 0, 5,  6, {  6,  6,  6 } }, // 00010
	{ 0, 0,  0, {  0,  0,  0 } }, // -
};

static uint16_t unquantize(uint16_t _value, bool _signed, uint8_t _endpointBits)
{
	const uint16_t maxValue = 1 << (_endpointBits - 1);

	if (_signed)
	{
		if (_endpointBits >= 16)
		{
			return _value;
		}

		const bool sign = !!(_value & 0x8000);
		_value &= 0x7fff;

		uint16_t unq;

		if (0 == _value)
		{
			unq = 0;
		}
		else if (_value >= maxValue - 1)
		{
			unq = 0x7fff;
		}
		else
		{
			unq = ((_value << 15) + 0x4000) >> (_endpointBits - 1);
		}

		return sign ? -unq : unq;
	}

	if (_endpointBits >= 15)
	{
		return _value;
	}

	if (0 == _value)
	{
		return 0;
	}

	if (_value == maxValue)
	{
		return UINT16_MAX;
	}

	return ((_value << 15) + 0x4000) >> (_endpointBits - 1);
}

static uint16_t finish_unquantize(uint16_t _value, bool _signed)
{
	if (_signed)
	{
		const uint16_t sign = _value & 0x8000;
		_value &= 0x7fff;

		return ((_value * 31) >> 5) | sign;
	}

	return (_value * 31) >> 6;
}

static uint16_t sign_extend(uint16_t _value, uint8_t _numBits)
{
	const uint16_t mask = 1 << (_numBits - 1);
	const uint16_t result = (_value ^ mask) - mask;

	return result;
}

static inline uint8_t f32_to_u8(const float f) {
	float c = roundf(f * 255);
	if (c < 0)
		return 0;
	else if (c > 255)
		return 255;
	else
		return c;
}

static uint8_t half_to_u8(uint16_t h) {
	return f32_to_u8(fp16_ieee_to_fp32_value(h));
}

static void decode_bc6_block(const uint8_t* _src, uint32_t* _dst, bool _signed)
{
	BitReader bit(_src);

	uint8_t mode = uint8_t(bit.read(2));

	uint16_t epR[4] = { /* rw, rx, ry, rz */ };
	uint16_t epG[4] = { /* gw, gx, gy, gz */ };
	uint16_t epB[4] = { /* bw, bx, by, bz */ };

	if (mode & 2)
	{
		// 5-bit mode
		mode |= bit.read(3) << 2;

		if (0 == s_bc6hModeInfo[mode].endpointBits)
		{
			memset(_dst, 0, 16 * 4);
			return;
		}

		switch (mode)
		{
		case 2:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(5) << 0;
			epR[0] |= bit.read(1) << 10;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(4) << 0;
			epG[0] |= bit.read(1) << 10;
			epB[3] |= bit.read(1) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(4) << 0;
			epB[0] |= bit.read(1) << 10;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 3;
			break;

		case 3:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(10) << 0;
			epG[1] |= bit.read(10) << 0;
			epB[1] |= bit.read(10) << 0;
			break;

		case 6:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(4) << 0;
			epR[0] |= bit.read(1) << 10;
			epG[3] |= bit.read(1) << 4;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(5) << 0;
			epG[0] |= bit.read(1) << 10;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(4) << 0;
			epB[0] |= bit.read(1) << 10;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(4) << 0;
			epB[3] |= bit.read(1) << 0;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(4) << 0;
			epG[2] |= bit.read(1) << 4;
			epB[3] |= bit.read(1) << 3;
			break;

		case 7:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(9) << 0;
			epR[0] |= bit.read(1) << 10;
			epG[1] |= bit.read(9) << 0;
			epG[0] |= bit.read(1) << 10;
			epB[1] |= bit.read(9) << 0;
			epB[0] |= bit.read(1) << 10;
			break;

		case 10:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(4) << 0;
			epR[0] |= bit.read(1) << 10;
			epB[2] |= bit.read(1) << 4;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(4) << 0;
			epG[0] |= bit.read(1) << 10;
			epB[3] |= bit.read(1) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(5) << 0;
			epB[0] |= bit.read(1) << 10;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(4) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(4) << 0;
			epB[3] |= bit.read(1) << 4;
			epB[3] |= bit.read(1) << 3;
			break;

		case 11:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(8) << 0;
			epR[0] |= bit.read(1) << 11;
			epR[0] |= bit.read(1) << 10;
			epG[1] |= bit.read(8) << 0;
			epG[0] |= bit.read(1) << 11;
			epG[0] |= bit.read(1) << 10;
			epB[1] |= bit.read(8) << 0;
			epB[0] |= bit.read(1) << 11;
			epB[0] |= bit.read(1) << 10;
			break;

		case 14:
			epR[0] |= bit.read(9) << 0;
			epB[2] |= bit.read(1) << 4;
			epG[0] |= bit.read(9) << 0;
			epG[2] |= bit.read(1) << 4;
			epB[0] |= bit.read(9) << 0;
			epB[3] |= bit.read(1) << 4;
			epR[1] |= bit.read(5) << 0;
			epG[3] |= bit.read(1) << 4;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 3;
			break;

		case 15:
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(4) << 0;
			epR[0] |= bit.read(1) << 15;
			epR[0] |= bit.read(1) << 14;
			epR[0] |= bit.read(1) << 13;
			epR[0] |= bit.read(1) << 12;
			epR[0] |= bit.read(1) << 11;
			epR[0] |= bit.read(1) << 10;
			epG[1] |= bit.read(4) << 0;
			epG[0] |= bit.read(1) << 15;
			epG[0] |= bit.read(1) << 14;
			epG[0] |= bit.read(1) << 13;
			epG[0] |= bit.read(1) << 12;
			epG[0] |= bit.read(1) << 11;
			epG[0] |= bit.read(1) << 10;
			epB[1] |= bit.read(4) << 0;
			epB[0] |= bit.read(1) << 15;
			epB[0] |= bit.read(1) << 14;
			epB[0] |= bit.read(1) << 13;
			epB[0] |= bit.read(1) << 12;
			epB[0] |= bit.read(1) << 11;
			epB[0] |= bit.read(1) << 10;
			break;

		case 18:
			epR[0] |= bit.read(8) << 0;
			epG[3] |= bit.read(1) << 4;
			epB[2] |= bit.read(1) << 4;
			epG[0] |= bit.read(8) << 0;
			epB[3] |= bit.read(1) << 2;
			epG[2] |= bit.read(1) << 4;
			epB[0] |= bit.read(8) << 0;
			epB[3] |= bit.read(1) << 3;
			epB[3] |= bit.read(1) << 4;
			epR[1] |= bit.read(6) << 0;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(6) << 0;
			epR[3] |= bit.read(6) << 0;
			break;

		case 22:
			epR[0] |= bit.read(8) << 0;
			epB[3] |= bit.read(1) << 0;
			epB[2] |= bit.read(1) << 4;
			epG[0] |= bit.read(8) << 0;
			epG[2] |= bit.read(1) << 5;
			epG[2] |= bit.read(1) << 4;
			epB[0] |= bit.read(8) << 0;
			epG[3] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 4;
			epR[1] |= bit.read(5) << 0;
			epG[3] |= bit.read(1) << 4;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(6) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 3;
			break;

		case 26:
			epR[0] |= bit.read(8) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(1) << 4;
			epG[0] |= bit.read(8) << 0;
			epB[2] |= bit.read(1) << 5;
			epG[2] |= bit.read(1) << 4;
			epB[0] |= bit.read(8) << 0;
			epB[3] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 4;
			epR[1] |= bit.read(5) << 0;
			epG[3] |= bit.read(1) << 4;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(6) << 0;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 3;
			break;

		case 30:
			epR[0] |= bit.read(6) << 0;
			epG[3] |= bit.read(1) << 4;
			epB[3] |= bit.read(1) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(1) << 4;
			epG[0] |= bit.read(6) << 0;
			epG[2] |= bit.read(1) << 5;
			epB[2] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 2;
			epG[2] |= bit.read(1) << 4;
			epB[0] |= bit.read(6) << 0;
			epG[3] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 3;
			epB[3] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 4;
			epR[1] |= bit.read(6) << 0;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(6) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(6) << 0;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(6) << 0;
			epR[3] |= bit.read(6) << 0;
			break;

		default:
			break;
		}
	}
	else
	{
		switch (mode)
		{
		case 0:
			epG[2] |= bit.read(1) << 4;
			epB[2] |= bit.read(1) << 4;
			epB[3] |= bit.read(1) << 4;
			epR[0] |= bit.read(10) << 0;
			epG[0] |= bit.read(10) << 0;
			epB[0] |= bit.read(10) << 0;
			epR[1] |= bit.read(5) << 0;
			epG[3] |= bit.read(1) << 4;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 2;
			epR[3] |= bit.read(5) << 0;
			epB[3] |= bit.read(1) << 3;
			break;

		case 1:
			epG[2] |= bit.read(1) << 5;
			epG[3] |= bit.read(1) << 4;
			epG[3] |= bit.read(1) << 5;
			epR[0] |= bit.read(7) << 0;
			epB[3] |= bit.read(1) << 0;
			epB[3] |= bit.read(1) << 1;
			epB[2] |= bit.read(1) << 4;
			epG[0] |= bit.read(7) << 0;
			epB[2] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 2;
			epG[2] |= bit.read(1) << 4;
			epB[0] |= bit.read(7) << 0;
			epB[3] |= bit.read(1) << 3;
			epB[3] |= bit.read(1) << 5;
			epB[3] |= bit.read(1) << 4;
			epR[1] |= bit.read(6) << 0;
			epG[2] |= bit.read(4) << 0;
			epG[1] |= bit.read(6) << 0;
			epG[3] |= bit.read(4) << 0;
			epB[1] |= bit.read(6) << 0;
			epB[2] |= bit.read(4) << 0;
			epR[2] |= bit.read(6) << 0;
			epR[3] |= bit.read(6) << 0;
			break;

		default:
			break;
		}
	}

	const Bc6hModeInfo mi = s_bc6hModeInfo[mode];

	if (_signed)
	{
		epR[0] = sign_extend(epR[0], mi.endpointBits);
		epG[0] = sign_extend(epG[0], mi.endpointBits);
		epB[0] = sign_extend(epB[0], mi.endpointBits);
	}

	const uint8_t numSubsets = !!mi.partitionBits + 1;

	for (uint8_t ii = 1, num = numSubsets * 2; ii < num; ++ii)
	{
		if (_signed
			|| mi.transformed)
		{
			epR[ii] = sign_extend(epR[ii], mi.deltaBits[0]);
			epG[ii] = sign_extend(epG[ii], mi.deltaBits[1]);
			epB[ii] = sign_extend(epB[ii], mi.deltaBits[2]);
		}

		if (mi.transformed)
		{
			const uint16_t mask = (1 << mi.endpointBits) - 1;

			epR[ii] = (epR[ii] + epR[0]) & mask;
			epG[ii] = (epG[ii] + epG[0]) & mask;
			epB[ii] = (epB[ii] + epB[0]) & mask;

			if (_signed)
			{
				epR[ii] = sign_extend(epR[ii], mi.endpointBits);
				epG[ii] = sign_extend(epG[ii], mi.endpointBits);
				epB[ii] = sign_extend(epB[ii], mi.endpointBits);
			}
		}
	}

	for (uint8_t ii = 0, num = numSubsets * 2; ii < num; ++ii)
	{
		epR[ii] = unquantize(epR[ii], _signed, mi.endpointBits);
		epG[ii] = unquantize(epG[ii], _signed, mi.endpointBits);
		epB[ii] = unquantize(epB[ii], _signed, mi.endpointBits);
	}

	const uint8_t partitionSetIdx = uint8_t(mi.partitionBits ? bit.read(5) : 0);
	const uint8_t indexBits = mi.partitionBits ? 3 : 4;
	const uint8_t* factors = s_bptcFactors[indexBits - 2];

	for (uint8_t yy = 0; yy < 4; ++yy)
	{
		for (uint8_t xx = 0; xx < 4; ++xx)
		{
			const uint8_t idx = yy * 4 + xx;

			uint8_t subsetIndex = 0;
			uint8_t indexAnchor = 0;

			if (0 != mi.partitionBits)
			{
				subsetIndex = (s_bptcP2[partitionSetIdx] >> idx) & 1;
				indexAnchor = subsetIndex ? s_bptcA2[partitionSetIdx] : 0;
			}

			const uint8_t anchor = idx == indexAnchor;
			const uint8_t num = indexBits - anchor;
			const uint8_t index = (uint8_t)bit.read(num);

			const uint8_t fc = factors[index];
			const uint8_t fca = 64 - fc;
			const uint8_t fcb = fc;

			subsetIndex *= 2;
			uint16_t rr = finish_unquantize((epR[subsetIndex] * fca + epR[subsetIndex + 1] * fcb + 32) >> 6, _signed);
			uint16_t gg = finish_unquantize((epG[subsetIndex] * fca + epG[subsetIndex + 1] * fcb + 32) >> 6, _signed);
			uint16_t bb = finish_unquantize((epB[subsetIndex] * fca + epB[subsetIndex + 1] * fcb + 32) >> 6, _signed);

			_dst[idx] = color(half_to_u8(rr), half_to_u8(gg), half_to_u8(bb), 255);
		}
	}
}

int decode_bc6(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image) {
	uint32_t m_block_width = 4;
	uint32_t m_block_height = 4;
	uint32_t m_blocks_x = (m_width + m_block_width - 1) / m_block_width;
	uint32_t m_blocks_y = (m_height + m_block_height - 1) / m_block_height;
	uint32_t buffer[16];
	for (uint32_t by = 0; by < m_blocks_y; by++) {
		for (uint32_t bx = 0; bx < m_blocks_x; bx++, data += 16) {
			decode_bc6_block(data, buffer, false);
			copy_block_buffer(bx, by, m_width, m_height, m_block_width, m_block_height, buffer, image);
		}
	}
	return 1;
}

static const uint32_t s_bptcP3[] =
{ //  76543210     0000   1111   2222   3333   4444   5555   6666   7777
	0xaa685050, // 0, 0,  1, 1,  0, 0,  1, 1,  0, 2,  2, 1,  2, 2,  2, 2
	0x6a5a5040, // 0, 0,  0, 1,  0, 0,  1, 1,  2, 2,  1, 1,  2, 2,  2, 1
	0x5a5a4200, // 0, 0,  0, 0,  2, 0,  0, 1,  2, 2,  1, 1,  2, 2,  1, 1
	0x5450a0a8, // 0, 2,  2, 2,  0, 0,  2, 2,  0, 0,  1, 1,  0, 1,  1, 1
	0xa5a50000, // 0, 0,  0, 0,  0, 0,  0, 0,  1, 1,  2, 2,  1, 1,  2, 2
	0xa0a05050, // 0, 0,  1, 1,  0, 0,  1, 1,  0, 0,  2, 2,  0, 0,  2, 2
	0x5555a0a0, // 0, 0,  2, 2,  0, 0,  2, 2,  1, 1,  1, 1,  1, 1,  1, 1
	0x5a5a5050, // 0, 0,  1, 1,  0, 0,  1, 1,  2, 2,  1, 1,  2, 2,  1, 1
	0xaa550000, // 0, 0,  0, 0,  0, 0,  0, 0,  1, 1,  1, 1,  2, 2,  2, 2
	0xaa555500, // 0, 0,  0, 0,  1, 1,  1, 1,  1, 1,  1, 1,  2, 2,  2, 2
	0xaaaa5500, // 0, 0,  0, 0,  1, 1,  1, 1,  2, 2,  2, 2,  2, 2,  2, 2
	0x90909090, // 0, 0,  1, 2,  0, 0,  1, 2,  0, 0,  1, 2,  0, 0,  1, 2
	0x94949494, // 0, 1,  1, 2,  0, 1,  1, 2,  0, 1,  1, 2,  0, 1,  1, 2
	0xa4a4a4a4, // 0, 1,  2, 2,  0, 1,  2, 2,  0, 1,  2, 2,  0, 1,  2, 2
	0xa9a59450, // 0, 0,  1, 1,  0, 1,  1, 2,  1, 1,  2, 2,  1, 2,  2, 2
	0x2a0a4250, // 0, 0,  1, 1,  2, 0,  0, 1,  2, 2,  0, 0,  2, 2,  2, 0
	0xa5945040, // 0, 0,  0, 1,  0, 0,  1, 1,  0, 1,  1, 2,  1, 1,  2, 2
	0x0a425054, // 0, 1,  1, 1,  0, 0,  1, 1,  2, 0,  0, 1,  2, 2,  0, 0
	0xa5a5a500, // 0, 0,  0, 0,  1, 1,  2, 2,  1, 1,  2, 2,  1, 1,  2, 2
	0x55a0a0a0, // 0, 0,  2, 2,  0, 0,  2, 2,  0, 0,  2, 2,  1, 1,  1, 1
	0xa8a85454, // 0, 1,  1, 1,  0, 1,  1, 1,  0, 2,  2, 2,  0, 2,  2, 2
	0x6a6a4040, // 0, 0,  0, 1,  0, 0,  0, 1,  2, 2,  2, 1,  2, 2,  2, 1
	0xa4a45000, // 0, 0,  0, 0,  0, 0,  1, 1,  0, 1,  2, 2,  0, 1,  2, 2
	0x1a1a0500, // 0, 0,  0, 0,  1, 1,  0, 0,  2, 2,  1, 0,  2, 2,  1, 0
	0x0050a4a4, // 0, 1,  2, 2,  0, 1,  2, 2,  0, 0,  1, 1,  0, 0,  0, 0
	0xaaa59090, // 0, 0,  1, 2,  0, 0,  1, 2,  1, 1,  2, 2,  2, 2,  2, 2
	0x14696914, // 0, 1,  1, 0,  1, 2,  2, 1,  1, 2,  2, 1,  0, 1,  1, 0
	0x69691400, // 0, 0,  0, 0,  0, 1,  1, 0,  1, 2,  2, 1,  1, 2,  2, 1
	0xa08585a0, // 0, 0,  2, 2,  1, 1,  0, 2,  1, 1,  0, 2,  0, 0,  2, 2
	0xaa821414, // 0, 1,  1, 0,  0, 1,  1, 0,  2, 0,  0, 2,  2, 2,  2, 2
	0x50a4a450, // 0, 0,  1, 1,  0, 1,  2, 2,  0, 1,  2, 2,  0, 0,  1, 1
	0x6a5a0200, // 0, 0,  0, 0,  2, 0,  0, 0,  2, 2,  1, 1,  2, 2,  2, 1
	0xa9a58000, // 0, 0,  0, 0,  0, 0,  0, 2,  1, 1,  2, 2,  1, 2,  2, 2
	0x5090a0a8, // 0, 2,  2, 2,  0, 0,  2, 2,  0, 0,  1, 2,  0, 0,  1, 1
	0xa8a09050, // 0, 0,  1, 1,  0, 0,  1, 2,  0, 0,  2, 2,  0, 2,  2, 2
	0x24242424, // 0, 1,  2, 0,  0, 1,  2, 0,  0, 1,  2, 0,  0, 1,  2, 0
	0x00aa5500, // 0, 0,  0, 0,  1, 1,  1, 1,  2, 2,  2, 2,  0, 0,  0, 0
	0x24924924, // 0, 1,  2, 0,  1, 2,  0, 1,  2, 0,  1, 2,  0, 1,  2, 0
	0x24499224, // 0, 1,  2, 0,  2, 0,  1, 2,  1, 2,  0, 1,  0, 1,  2, 0
	0x50a50a50, // 0, 0,  1, 1,  2, 2,  0, 0,  1, 1,  2, 2,  0, 0,  1, 1
	0x500aa550, // 0, 0,  1, 1,  1, 1,  2, 2,  2, 2,  0, 0,  0, 0,  1, 1
	0xaaaa4444, // 0, 1,  0, 1,  0, 1,  0, 1,  2, 2,  2, 2,  2, 2,  2, 2
	0x66660000, // 0, 0,  0, 0,  0, 0,  0, 0,  2, 1,  2, 1,  2, 1,  2, 1
	0xa5a0a5a0, // 0, 0,  2, 2,  1, 1,  2, 2,  0, 0,  2, 2,  1, 1,  2, 2
	0x50a050a0, // 0, 0,  2, 2,  0, 0,  1, 1,  0, 0,  2, 2,  0, 0,  1, 1
	0x69286928, // 0, 2,  2, 0,  1, 2,  2, 1,  0, 2,  2, 0,  1, 2,  2, 1
	0x44aaaa44, // 0, 1,  0, 1,  2, 2,  2, 2,  2, 2,  2, 2,  0, 1,  0, 1
	0x66666600, // 0, 0,  0, 0,  2, 1,  2, 1,  2, 1,  2, 1,  2, 1,  2, 1
	0xaa444444, // 0, 1,  0, 1,  0, 1,  0, 1,  0, 1,  0, 1,  2, 2,  2, 2
	0x54a854a8, // 0, 2,  2, 2,  0, 1,  1, 1,  0, 2,  2, 2,  0, 1,  1, 1
	0x95809580, // 0, 0,  0, 2,  1, 1,  1, 2,  0, 0,  0, 2,  1, 1,  1, 2
	0x96969600, // 0, 0,  0, 0,  2, 1,  1, 2,  2, 1,  1, 2,  2, 1,  1, 2
	0xa85454a8, // 0, 2,  2, 2,  0, 1,  1, 1,  0, 1,  1, 1,  0, 2,  2, 2
	0x80959580, // 0, 0,  0, 2,  1, 1,  1, 2,  1, 1,  1, 2,  0, 0,  0, 2
	0xaa141414, // 0, 1,  1, 0,  0, 1,  1, 0,  0, 1,  1, 0,  2, 2,  2, 2
	0x96960000, // 0, 0,  0, 0,  0, 0,  0, 0,  2, 1,  1, 2,  2, 1,  1, 2
	0xaaaa1414, // 0, 1,  1, 0,  0, 1,  1, 0,  2, 2,  2, 2,  2, 2,  2, 2
	0xa05050a0, // 0, 0,  2, 2,  0, 0,  1, 1,  0, 0,  1, 1,  0, 0,  2, 2
	0xa0a5a5a0, // 0, 0,  2, 2,  1, 1,  2, 2,  1, 1,  2, 2,  0, 0,  2, 2
	0x96000000, // 0, 0,  0, 0,  0, 0,  0, 0,  0, 0,  0, 0,  2, 1,  1, 2
	0x40804080, // 0, 0,  0, 2,  0, 0,  0, 1,  0, 0,  0, 2,  0, 0,  0, 1
	0xa9a8a9a8, // 0, 2,  2, 2,  1, 2,  2, 2,  0, 2,  2, 2,  1, 2,  2, 2
	0xaaaaaa44, // 0, 1,  0, 1,  2, 2,  2, 2,  2, 2,  2, 2,  2, 2,  2, 2
	0x2a4a5254, // 0, 1,  1, 1,  2, 0,  1, 1,  2, 2,  0, 1,  2, 2,  2, 0
};

static const uint8_t s_bptcA3[2][64] =
{
	{
		 3,  3, 15, 15,  8,  3, 15, 15,
		 8,  8,  6,  6,  6,  5,  3,  3,
		 3,  3,  8, 15,  3,  3,  6, 10,
		 5,  8,  8,  6,  8,  5, 15, 15,
		 8, 15,  3,  5,  6, 10,  8, 15,
		15,  3, 15,  5, 15, 15, 15, 15,
		 3, 15,  5,  5,  5,  8,  5, 10,
		 5, 10,  8, 13, 15, 12,  3,  3,
	},
	{
		15,  8,  8,  3, 15, 15,  3,  8,
		15, 15, 15, 15, 15, 15, 15,  8,
		15,  8, 15,  3, 15,  8, 15,  8,
		 3, 15,  6, 10, 15, 15, 10,  8,
		15,  3, 15, 10, 10,  8,  9, 10,
		 6, 15,  8, 15,  3,  6,  6,  8,
		15,  3, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15,  3, 15, 15,  8,
	},
};

struct Bc7ModeInfo
{
	uint8_t numSubsets;
	uint8_t partitionBits;
	uint8_t rotationBits;
	uint8_t indexSelectionBits;
	uint8_t colorBits;
	uint8_t alphaBits;
	uint8_t endpointPBits;
	uint8_t sharedPBits;
	uint8_t indexBits[2];
};

static const Bc7ModeInfo s_bp7ModeInfo[] =
{ //  +---------------------------- num subsets
  //  |  +------------------------- partition bits
  //  |  |  +---------------------- rotation bits
  //  |  |  |  +------------------- index selection bits
  //  |  |  |  |  +---------------- color bits
  //  |  |  |  |  |  +------------- alpha bits
  //  |  |  |  |  |  |  +---------- endpoint P-bits
  //  |  |  |  |  |  |  |  +------- shared P-bits
  //  |  |  |  |  |  |  |  |    +-- 2x index bits
	{ 3, 4, 0, 0, 4, 0, 1, 0, { 3, 0 } }, // 0
	{ 2, 6, 0, 0, 6, 0, 0, 1, { 3, 0 } }, // 1
	{ 3, 6, 0, 0, 5, 0, 0, 0, { 2, 0 } }, // 2
	{ 2, 6, 0, 0, 7, 0, 1, 0, { 2, 0 } }, // 3
	{ 1, 0, 2, 1, 5, 6, 0, 0, { 2, 3 } }, // 4
	{ 1, 0, 2, 0, 7, 8, 0, 0, { 2, 2 } }, // 5
	{ 1, 0, 0, 0, 7, 7, 1, 0, { 4, 0 } }, // 6
	{ 2, 6, 0, 0, 5, 5, 1, 0, { 2, 0 } }, // 7
};

static uint8_t expand_quantized(uint8_t v, int bits) {
	v = v << (8 - bits);
	return v | (v >> bits);
}

static void decode_bc7_block(const uint8_t* _src, uint32_t* _dst)
{
	BitReader bit(_src);

	uint8_t mode = 0;
	for (; mode < 8 && 0 == bit.read(1); ++mode)
	{
	}

	if (mode == 8)
	{
		memset(_dst, 0, 16 * 4);
		return;
	}

	const Bc7ModeInfo& mi = s_bp7ModeInfo[mode];
	const uint8_t modePBits = 0 != mi.endpointPBits
		? mi.endpointPBits
		: mi.sharedPBits
		;

	const uint8_t partitionSetIdx = uint8_t(bit.read(mi.partitionBits));
	const uint8_t rotationMode = uint8_t(bit.read(mi.rotationBits));
	const uint8_t indexSelectionMode = uint8_t(bit.read(mi.indexSelectionBits));

	uint8_t epR[6];
	uint8_t epG[6];
	uint8_t epB[6];
	uint8_t epA[6];

	for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
	{
		epR[ii * 2 + 0] = uint8_t(bit.read(mi.colorBits) << modePBits);
		epR[ii * 2 + 1] = uint8_t(bit.read(mi.colorBits) << modePBits);
	}

	for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
	{
		epG[ii * 2 + 0] = uint8_t(bit.read(mi.colorBits) << modePBits);
		epG[ii * 2 + 1] = uint8_t(bit.read(mi.colorBits) << modePBits);
	}

	for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
	{
		epB[ii * 2 + 0] = uint8_t(bit.read(mi.colorBits) << modePBits);
		epB[ii * 2 + 1] = uint8_t(bit.read(mi.colorBits) << modePBits);
	}

	if (mi.alphaBits)
	{
		for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
		{
			epA[ii * 2 + 0] = uint8_t(bit.read(mi.alphaBits) << modePBits);
			epA[ii * 2 + 1] = uint8_t(bit.read(mi.alphaBits) << modePBits);
		}
	}
	else
	{
		memset(epA, 0xff, 6);
	}

	if (0 != modePBits)
	{
		for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
		{
			const uint8_t pda = uint8_t(bit.read(modePBits));
			const uint8_t pdb = uint8_t(0 == mi.sharedPBits ? bit.read(modePBits) : pda);

			epR[ii * 2 + 0] |= pda;
			epR[ii * 2 + 1] |= pdb;
			epG[ii * 2 + 0] |= pda;
			epG[ii * 2 + 1] |= pdb;
			epB[ii * 2 + 0] |= pda;
			epB[ii * 2 + 1] |= pdb;
			epA[ii * 2 + 0] |= pda;
			epA[ii * 2 + 1] |= pdb;
		}
	}

	const uint8_t colorBits = mi.colorBits + modePBits;

	for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
	{
		epR[ii * 2 + 0] = expand_quantized(epR[ii * 2 + 0], colorBits);
		epR[ii * 2 + 1] = expand_quantized(epR[ii * 2 + 1], colorBits);
		epG[ii * 2 + 0] = expand_quantized(epG[ii * 2 + 0], colorBits);
		epG[ii * 2 + 1] = expand_quantized(epG[ii * 2 + 1], colorBits);
		epB[ii * 2 + 0] = expand_quantized(epB[ii * 2 + 0], colorBits);
		epB[ii * 2 + 1] = expand_quantized(epB[ii * 2 + 1], colorBits);
	}

	if (mi.alphaBits)
	{
		const uint8_t alphaBits = mi.alphaBits + modePBits;

		for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
		{
			epA[ii * 2 + 0] = expand_quantized(epA[ii * 2 + 0], alphaBits);
			epA[ii * 2 + 1] = expand_quantized(epA[ii * 2 + 1], alphaBits);
		}
	}

	const bool hasIndexBits1 = 0 != mi.indexBits[1];

	const uint8_t* factors[] =
	{
						s_bptcFactors[mi.indexBits[0] - 2],
		hasIndexBits1 ? s_bptcFactors[mi.indexBits[1] - 2] : factors[0],
	};

	uint16_t offset[2] =
	{
		0,
		uint16_t(mi.numSubsets * (16 * mi.indexBits[0] - 1)),
	};

	for (uint8_t yy = 0; yy < 4; ++yy)
	{
		for (uint8_t xx = 0; xx < 4; ++xx)
		{
			const uint8_t idx = yy * 4 + xx;

			uint8_t subsetIndex = 0;
			uint8_t indexAnchor = 0;
			switch (mi.numSubsets)
			{
			case 2:
				subsetIndex = (s_bptcP2[partitionSetIdx] >> idx) & 1;
				indexAnchor = 0 != subsetIndex ? s_bptcA2[partitionSetIdx] : 0;
				break;

			case 3:
				subsetIndex = (s_bptcP3[partitionSetIdx] >> (2 * idx)) & 3;
				indexAnchor = 0 != subsetIndex ? s_bptcA3[subsetIndex - 1][partitionSetIdx] : 0;
				break;

			default:
				break;
			}

			const uint8_t anchor = idx == indexAnchor;
			const uint8_t num[2] =
			{
				uint8_t(mi.indexBits[0] - anchor),
				uint8_t(hasIndexBits1 ? mi.indexBits[1] - anchor : 0),
			};

			const uint8_t index[2] =
			{
								(uint8_t)bit.peek(offset[0], num[0]),
				hasIndexBits1 ? (uint8_t)bit.peek(offset[1], num[1]) : index[0],
			};

			offset[0] += num[0];
			offset[1] += num[1];

			const uint8_t fc = factors[indexSelectionMode][index[indexSelectionMode]];
			const uint8_t fa = factors[!indexSelectionMode][index[!indexSelectionMode]];

			const uint8_t fca = 64 - fc;
			const uint8_t fcb = fc;
			const uint8_t faa = 64 - fa;
			const uint8_t fab = fa;

			subsetIndex *= 2;
			uint8_t rr = uint8_t(uint16_t(epR[subsetIndex] * fca + epR[subsetIndex + 1] * fcb + 32) >> 6);
			uint8_t gg = uint8_t(uint16_t(epG[subsetIndex] * fca + epG[subsetIndex + 1] * fcb + 32) >> 6);
			uint8_t bb = uint8_t(uint16_t(epB[subsetIndex] * fca + epB[subsetIndex + 1] * fcb + 32) >> 6);
			uint8_t aa = uint8_t(uint16_t(epA[subsetIndex] * faa + epA[subsetIndex + 1] * fab + 32) >> 6);

			switch (rotationMode)
			{
			case 1: std::swap(aa, rr); break;
			case 2: std::swap(aa, gg); break;
			case 3: std::swap(aa, bb); break;
			default:                  break;
			};

			_dst[idx] = color(rr, gg, bb, aa);
		}
	}
}

int decode_bc7(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image) {
	uint32_t m_block_width = 4;
	uint32_t m_block_height = 4;
	uint32_t m_blocks_x = (m_width + m_block_width - 1) / m_block_width;
	uint32_t m_blocks_y = (m_height + m_block_height - 1) / m_block_height;
	uint32_t buffer[16];
	for (uint32_t by = 0; by < m_blocks_y; by++) {
		for (uint32_t bx = 0; bx < m_blocks_x; bx++, data += 16) {
			decode_bc7_block(data, buffer);
			copy_block_buffer(bx, by, m_width, m_height, m_block_width, m_block_height, buffer, image);
		}
	}
	return 1;
}
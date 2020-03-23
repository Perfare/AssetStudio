#include <string.h>
#include "Texture2DDecoder.h"
#include "bcn.h"
#include "pvrtc.h"
#include "etc.h"
#include "atc.h"
#include "astc.h"
#include "crunch.h"
#include "unitycrunch.h"

namespace Texture2DDecoder {
	bool TextureDecoder::DecodeDXT1(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_bc1(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeDXT5(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_bc3(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodePVRTC(array<Byte>^ data, long w, long h, array<Byte>^ image, bool is2bpp) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_pvrtc(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin), is2bpp ? 1 : 0);
	}

	bool TextureDecoder::DecodeETC1(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_etc1(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeETC2(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_etc2(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeETC2A1(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_etc2a1(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeETC2A8(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_etc2a8(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeEACR(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_eacr(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeEACRSigned(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_eacr_signed(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeEACRG(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_eacrg(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeEACRGSigned(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_eacrg_signed(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeBC4(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_bc4(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeBC5(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_bc5(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeBC6(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_bc6(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeBC7(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_bc7(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeATCRGB4(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_atc_rgb4(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeATCRGBA8(array<Byte>^ data, long w, long h, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_atc_rgba8(dataPin, w, h, reinterpret_cast<uint32_t*>(imagePin));
	}

	bool TextureDecoder::DecodeASTC(array<Byte>^ data, long w, long h, int bw, int bh, array<Byte>^ image) {
		pin_ptr<unsigned char> dataPin = &data[0];
		pin_ptr<unsigned char> imagePin = &image[0];
		return decode_astc(dataPin, w, h, bw, bh, reinterpret_cast<uint32_t*>(imagePin));
	}

	array<Byte>^ TextureDecoder::UnpackCrunch(array<Byte>^ data) {
		pin_ptr<unsigned char> dataPin = &data[0];
		void* ret;
		uint32_t retSize;
		if (!crunch_unpack_level(dataPin, data->Length, 0, &ret, &retSize)) {
			return nullptr;
		}
		auto buff = gcnew array<Byte>(retSize);
		pin_ptr<unsigned char> buffPin = &buff[0];
		memcpy(buffPin, ret, retSize);
		delete ret;
		return buff;
	}

	array<Byte>^ TextureDecoder::UnpackUnityCrunch(array<Byte>^ data) {
		pin_ptr<unsigned char> dataPin = &data[0];
		void* ret;
		uint32_t retSize;
		if (!unity_crunch_unpack_level(dataPin, data->Length, 0, &ret, &retSize)) {
			return nullptr;
		}
		auto buff = gcnew array<Byte>(retSize);
		pin_ptr<unsigned char> buffPin = &buff[0];
		memcpy(buffPin, ret, retSize);
		delete ret;
		return buff;
	}
}


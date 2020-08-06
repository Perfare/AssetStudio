#pragma once

#if defined(_MSC_VER)
#if _MSC_VER < 1910 // MSVC 2017-
#error MSVC 2017 or later is required.
#endif
#endif

#if defined(WIN32) || defined(_WIN32) || defined(__CYGWIN__) || defined(__MINGW__)
#ifdef _AS_DLL
#ifdef __GNUC__
#define _AS_EXPORT __attribute__ ((dllexport))
#else
#define _AS_EXPORT __declspec(dllexport)
#endif
#else
#ifdef __GNUC__
#define _AS_EXPORT __attribute__ ((dllimport))
#else
#define _AS_EXPORT __declspec(dllimport)
#endif
#endif
#define _AS_LOCAL
#else
#if __GNUC__ >= 4
#define _AS_EXPORT __attribute__ ((visibility ("default")))
#define _AS_LOCAL  __attribute__ ((visibility ("hidden")))
#else
#define _AS_EXPORT
#define _AS_LOCAL
#endif
#endif

#ifdef __cplusplus
#ifndef _EXTERN_C_STMT
#define _EXTERN_C_STMT extern "C"
#endif
#else
#ifndef _EXTERN_C_STMT
#define _EXTERN_C_STMT
#endif
#endif

#ifndef _AS_CALL
#if defined(WIN32) || defined(_WIN32)
#define _AS_CALL __stdcall
#else
#define _AS_CALL /* __cdecl */
#endif
#endif

#if defined(_MSC_VER)
#define AS_API(ret_type) _EXTERN_C_STMT _AS_EXPORT ret_type _AS_CALL
#else
#define AS_API(ret_type) _EXTERN_C_STMT _AS_EXPORT _AS_CALL ret_type
#endif

#pragma once

#if defined(_MSC_VER)
#if _MSC_VER < 1910 // MSVC 2017-
#error MSVC 2017 or later is required.
#endif
#endif

#if defined(WIN32) || defined(_WIN32) || defined(__CYGWIN__) || defined(__MINGW__)
#ifdef _T2D_DLL
#ifdef __GNUC__
#define _T2D_EXPORT __attribute__ ((dllexport))
#else
#define _T2D_EXPORT __declspec(dllexport)
#endif
#else
#ifdef __GNUC__
#define _T2D_EXPORT __attribute__ ((dllimport))
#else
#define _T2D_EXPORT __declspec(dllimport)
#endif
#endif
#define _T2D_LOCAL
#else
#if __GNUC__ >= 4
#define _T2D_EXPORT __attribute__ ((visibility ("default")))
#define _T2D_LOCAL  __attribute__ ((visibility ("hidden")))
#else
#define _T2D_EXPORT
#define _T2D_LOCAL
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

#ifndef _T2D_CALL
#if defined(WIN32) || defined(_WIN32)
#define _T2D_CALL __stdcall
#else
#define _T2D_CALL /* __cdecl */
#endif
#endif

#if defined(_MSC_VER)
#define T2D_API(ret_type) _EXTERN_C_STMT _T2D_EXPORT ret_type _T2D_CALL
#else
#define T2D_API(ret_type) _EXTERN_C_STMT _T2D_EXPORT _T2D_CALL ret_type
#endif

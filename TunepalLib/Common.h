#include <Windows.h>

// Exception Helper Method
inline void THROW_IF_FAILED(HRESULT hr)
{
    if (FAILED(hr)) throw Platform::Exception::CreateException(hr);
}

// release and zero out a possible NULL pointer. note this will
// do the release on a temp copy to avoid reentrancy issues that can result from
// callbacks durring the release
template <class T> void SafeRelease(__deref_inout_opt T **ppT)
{
    T *pTTemp = *ppT;    // temp copy
    *ppT = nullptr;      // zero the input

    if (pTTemp) pTTemp->Release();
}

#ifndef SAFE_RELEASE
#define SAFE_RELEASE(x) { SafeRelease(&x); }
#endif

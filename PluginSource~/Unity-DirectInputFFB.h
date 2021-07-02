#include "pch.h"

#ifdef UNITYDIRECTINPTFFB_EXPORTS
#define UNITYDIRECTINPTFFB_API __declspec(dllexport)
#else
#define UNITYDIRECTINPTFFB_API __declspec(dllimport)
#endif

#define SAFE_DELETE(p)  { if(p) { delete (p);     (p)=NULL; } }

LPDIRECTINPUT8          g_pDI = NULL;
LPDIRECTINPUTDEVICE8    g_pDevice = NULL;
BOOL                    g_bActive = TRUE;
DWORD                   g_dwNumForceFeedbackAxis = 0;

BOOL CALLBACK _cbEnumFFBDevices(const DIDEVICEINSTANCE* pInst, void* pContext);
BOOL CALLBACK _cbEnumFFBAxes(const DIDEVICEOBJECTINSTANCE* pdidoi, void* pContext);

void ClearDeviceInstances();
void ClearDeviceAxes();
void FreeFFBDevice();
void FreeDirectInput();

extern "C"
{
   struct DeviceInfo {
      DWORD deviceType;
      LPSTR guidInstance;
      LPSTR guidProduct;
      LPSTR instanceName;
      LPSTR productName;
   };

   struct DeviceAxisInfo {
      DWORD offset;
      DWORD type;
      DWORD flags;
      DWORD ffMaxForce;
      DWORD ffForceResolution;
      DWORD collectionNumber;
      DWORD designatorIndex;
      DWORD usagePage;
      DWORD usage;
      DWORD dimension;
      DWORD exponent;
      DWORD reportId;
      LPSTR guidType;
      LPSTR name;
   };

   struct Effects {
      typedef enum {
         ConstantForce = 0,
         RampForce = 1,
         Square = 2,
         Sine = 3,
         Triangle = 4,
         SawtoothUp = 5,
         SawtoothDown = 6,
         Spring = 7,
         Damper = 8,
         Inertia = 9,
         Friction = 10,
         CustomForce = 11
      } Type;
   };

   UNITYDIRECTINPTFFB_API HRESULT         StartDirectInput();
   UNITYDIRECTINPTFFB_API DeviceInfo*     EnumerateFFBDevices(int &deviceCount);
   UNITYDIRECTINPTFFB_API DeviceAxisInfo* EnumerateFFBAxes(int &axisCount);
   UNITYDIRECTINPTFFB_API HRESULT         CreateFFBDevice(LPCSTR guidInstance);
   UNITYDIRECTINPTFFB_API HRESULT         AddFFBEffect(Effects::Type effectType);
   UNITYDIRECTINPTFFB_API HRESULT         UpdateEffectGain(Effects::Type effectType, float gainPercent);
   UNITYDIRECTINPTFFB_API HRESULT         GetDeviceState(DIJOYSTATE2 &m_deviceState);
   UNITYDIRECTINPTFFB_API HRESULT         UpdateConstantForce(LONG magnitude, LONG* directions);
   UNITYDIRECTINPTFFB_API HRESULT         UpdateSpringRaw(DICONDITION* conditions);
   UNITYDIRECTINPTFFB_API HRESULT         UpdateSpring(LONG Offset, LONG Coeff, LONG Saturation);
   UNITYDIRECTINPTFFB_API HRESULT         UpdateDamperRaw(DICONDITION* conditions);
   UNITYDIRECTINPTFFB_API HRESULT         UpdateDamper(LONG Magnitude);
   UNITYDIRECTINPTFFB_API HRESULT         SetAutoCenter(bool autoCenter);
   UNITYDIRECTINPTFFB_API void            StartAllFFBEffects();
   UNITYDIRECTINPTFFB_API void            StopAllFFBEffects();
   UNITYDIRECTINPTFFB_API void            StopDirectInput();
}

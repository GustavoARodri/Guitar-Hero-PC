// The FidelityFX Vulkan backend with Vulkan loaded through volk (the port
// Modified by GustavoARodri on 2026-10-27: Included the mutex header required by the bundled FidelityFX Vulkan backend.
// links no Vulkan loader; libvulkan / vulkan-1.dll is loaded at run time).
#include <mutex>
#include "volk.h"
#include "src/backends/vk/ffx_vk.cpp"

// The frame-interpolation swap chain is not built (FSR 3.1 upscaling only);
// ffxGetInterfaceVK still references this entry point.
FfxErrorCode ffxSetFrameGenerationConfigToSwapchainVK(FfxFrameGenerationConfig const*) { return FFX_ERROR_INVALID_ARGUMENT; }

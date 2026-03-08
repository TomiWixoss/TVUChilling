cmake_minimum_required(VERSION 3.10)
project(native-video-encoder)

# Require Android API 26+ for MediaCodec Surface input
add_compile_definitions(__ANDROID_API__=26)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -O3")

# Android NDK paths
set(ANDROID_NDK $ENV{ANDROID_NDK_ROOT})

# Source files
set(SOURCES
    src/video_encoder.cpp
    src/jni_bridge.cpp
)

# Create shared library
add_library(native-video-encoder SHARED ${SOURCES})

# Link libraries
target_link_libraries(native-video-encoder
    android
    log
    EGL
    GLESv3
    mediandk
)

# Include directories
target_include_directories(native-video-encoder PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

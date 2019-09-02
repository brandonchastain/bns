#include "util.h"

#define OCTET_SIZE 8

BYTE getMSBInUTF8(BYTE c) {
    BYTE msb = c & 0b10000000;
    if (msb == 0) {
        return '0';
    }
    return '1';
}

// populates output buffer with a string representation of the binary
// data that is passed in.
// size of output is assumed to be (dataSize * OCTET_SIZE) + 1
void tobinstr(BYTE* data, size_t dataSize, char* output) {
    
    for (int i = 0; i < dataSize; i++) {
        unsigned char byte = data[i];
        // printf("[%d]: %d\n", i, byte);

        for (int j = 0; j < OCTET_SIZE; j++) {
            int current = (i * OCTET_SIZE) + j;
            char digit = getMSBInUTF8(byte);
            output[current] = digit;
            byte = byte << 1;
        }
    }

    output[dataSize * OCTET_SIZE] = '\0';
}

// easy api to just print a binary string. Also shows example usage of tobinstr.
void printBinStr(unsigned char* buffer, size_t bufferSize) {
    unsigned char binstr[(bufferSize * 8) + 1]; // 8 bits printed per item in the buffer, plus null terminator
    tobinstr(buffer, bufferSize, binstr);
    printf("%s\n", binstr);
}
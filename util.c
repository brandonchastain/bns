#include <arpa/inet.h>
#include <assert.h>
#include "util.h"

#define OCTET_SIZE 8

// convert endianness where needed
void toNetworkOrder(Header* h) {
    h->identifier = htons(h->identifier);
    h->flags = htons(h->flags);
    h->questionCount = htons(h->questionCount);
    h->answerCount = htons(h->answerCount);
    h->authorityCount = htons(h->authorityCount);
    h->addtlCount = htons(h->addtlCount);
}

BYTE getMSBInUTF8(BYTE c) {
    BYTE msb = c & 0b10000000;
    if (msb == 0) {
        return '0';
    }
    return '1';
}

// populates output buffer with a string representation of the binary
// data that is passed in.
// size of output is assumed to be (dataSize) * (OCTET_SIZE + 1)
void tobinstr(BYTE* data, size_t dataSize, char* output) {
    int wordSize = OCTET_SIZE + 1; //byte plus a space
    for (int i = 0; i < dataSize; i++) {
        unsigned char byte = data[i];
        // printf("[%d]: %d\n", i, byte);

        int current;
        for (int j = 0; j < OCTET_SIZE; j++) {
            current = (i * wordSize) + j;
            char digit = getMSBInUTF8(byte);
            output[current] = digit;
            byte = byte << 1;
        }

        output[current + 1] = ' '; //add a space after each byte
    }

    output[dataSize * wordSize] = '\0';
}

// easy api to just print a binary string. Also shows example usage of tobinstr.
void printBinStr(unsigned char* buffer, size_t bufferSize) {
    unsigned char binstr[(bufferSize * 9)]; // 8 bits printed per item in the buffer, plus null terminator
    tobinstr(buffer, bufferSize, binstr);
    printf("%s\n", binstr);
}

void printHexStr(unsigned char* buffer, size_t bufferSize) {
    for (int i = 0; i < bufferSize; i++) {
        printf("%02x ", buffer[i]);
        if (i % 8 >= 7) {
            printf("\n");
        }
    }
    printf("\n");
}
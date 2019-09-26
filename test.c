#include <string.h>
#include "util.h"
#include "types.h"


void testLongDomain() {
    printf("%d\n", (int)strlen("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcde.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijk.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijk.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijk.com"));
}

void testBinaryHex() {
    unsigned char buffer[] = {0xff, 0x00};
    size_t bufferSize = sizeof(buffer);
    printBinStr((unsigned char *)&buffer, bufferSize);
    printHexStr((unsigned char*)&buffer, bufferSize);
}

void testBinaryHex2() {
    unsigned char buffer[] = {0x01, 0x01, 0x00, 0x02};
    size_t bufferSize = sizeof(buffer);
    printBinStr((unsigned char *)&buffer, bufferSize); 
    printHexStr((unsigned char*)&buffer, bufferSize);
}

void testByteOrder() {
    union {
        int i;
        char c[sizeof(int)];
    } foo;

    foo.i = 1;
    if (foo.c[0] == 1) {
        printf("System is little endian.\n");
    } else {
        printf("System is big endian.\n");
    }
}

void testToNetworkOrder() {
    uint32_t n = 44436u;
    printHexStr((BYTE *)&n, sizeof(n));
    toNetworkOrder((BYTE *)&n, sizeof(n));
    printHexStr((BYTE *)&n, sizeof(n));
}

int main(void) {
    testByteOrder();
    testToNetworkOrder();
    return 0;
}
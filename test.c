#include "util.h"


int main(void) {
    unsigned char buffer[] = {0xff, 0x00};
    size_t bufferSize = sizeof(buffer);
    printBinStr((unsigned char *)&buffer, bufferSize); 
    
    unsigned char buffer2[] = {0x01, 0x01, 0x00, 0x02};
    bufferSize = sizeof(buffer2);
    printBinStr((unsigned char *)&buffer2, bufferSize); 

    return 0;
}
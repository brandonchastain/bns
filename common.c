#include <assert.h>
#include <string.h>
#include "common.h"

void printHeader(Header *h) {
    assert(h != NULL);

    printf("HEADER\n");
    printf("\tid: %d\n", h->identifier);
    printf("\tqueryResponse: %d\n", h->queryResponse);
    printf("\topCode: %d\n", h->opCode);
    printf("\taa: %d\n", h->aa);
    printf("\ttc: %d\n", h->tc);
    printf("\trd: %d\n", h->rd);
    printf("\tra: %d\n", h->ra);
    printf("\tz: %d\n", h->z);
    printf("\trCode: %d\n", h->rCode);
    printf("\tquestionCount: %d\n", h->questionCount);
    printf("\tanswerCount: %d\n", h->answerCount);
    printf("\tauthorityCount: %d\n", h->authorityCount);
    printf("\taddtlCount: %d\n", h->addtlCount);
}

int parseDnsRequest(BYTE* buffer, size_t bufferSize) {
    if (bufferSize > 512) {
        printf("error: dns request is larger than allowed max size of 512 bytes");
        return -1;
    }

    Header h;
    memset(&h, 0, sizeof(h));

    h.identifier = buffer[0] << 8;
    h.identifier |= buffer[1];

    h.queryResponse = (buffer[2] >> 7);
    h.opCode = ((buffer[2] & 0x78)) >> 3;
    h.aa = ((buffer[2] & 0b00000100)) >> 2;
    h.tc = ((buffer[2] & 0b00000010)) >> 1;
    h.rd = ((buffer[2] & 0b00000001));
    h.ra = ((buffer[3] & 0b10000000)) >> 7;
    h.z = ((buffer[3] & 0b01110000)) >> 4;
    h.rCode = ((buffer[3] & 0x0f));

    h.questionCount = buffer[4] << 8;
    h.questionCount |= buffer[5];
    h.answerCount = buffer[6] << 8;
    h.answerCount |= buffer[7];
    h.authorityCount = buffer[8] << 8;
    h.authorityCount |= buffer[9];
    h.addtlCount = buffer[10] << 8;
    h.addtlCount = buffer[11];

    printHeader(&h);

    return 0;
}
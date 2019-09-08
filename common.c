#include <assert.h>
#include <string.h>
#include "common.h"

uint16_t parseHeader(Header* h, BYTE* rawRequest) {
    BYTE rawHeader[12];
    memcpy(&rawHeader, rawRequest, sizeof(rawHeader));

    h->identifier |= rawHeader[1];
    h->identifier = rawHeader[0] << 8;

    h->queryResponse = (rawHeader[2] >> 7);
    h->opCode = ((rawHeader[2] & 0x78)) >> 3;
    h->aa = ((rawHeader[2] & 0b00000100)) >> 2;
    h->tc = ((rawHeader[2] & 0b00000010)) >> 1;
    h->rd = ((rawHeader[2] & 0b00000001));
    h->ra = ((rawHeader[3] & 0b10000000)) >> 7;
    h->z = ((rawHeader[3] & 0b01110000)) >> 4;
    h->rCode = ((rawHeader[3] & 0x0f));

    h->questionCount = rawHeader[4] << 8;
    h->questionCount |= rawHeader[5];
    h->answerCount = rawHeader[6] << 8;
    h->answerCount |= rawHeader[7];
    h->authorityCount = rawHeader[8] << 8;
    h->authorityCount |= rawHeader[9];
    h->addtlCount = rawHeader[10] << 8;
    h->addtlCount = rawHeader[11];

    return sizeof(rawHeader);
}

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

uint16_t parseQuestion(Question* q, BYTE* rawRequest) {
    printf("reading qname...\n");
    uint16_t bytesRead = 0;

    BYTE* curr = rawRequest;
    BYTE name[QNAME_SIZE];
    BYTE* currOut = name;

    char dot = '.';
    while (*curr != 0) {
        BYTE labelLen = *curr;
        curr += 1;
        printf("copying label...\n");
        memcpy(currOut, curr, labelLen);
        currOut += labelLen;
        printf("copied. adding a dot...\n");
        memset(currOut, dot, sizeof(dot));
        currOut += sizeof(dot);
        printf("dot added.\n");

        curr += labelLen;
        bytesRead += (labelLen + 1);
        printf("read %d bytes so far: %s\n", bytesRead, name);
    }

    char nullTerm = '\0';
    memset(currOut, nullTerm, sizeof(nullTerm));
    currOut += sizeof(nullTerm);

    strcpy(q->qname, name);

    return bytesRead;
}

void printQuestion(Question* q) {
    printf("QUESTION\n");
    printf("\tqname: %s\n", q->qname);
}

int parseDnsRequest(BYTE* buffer, size_t bufferSize) {
    if (bufferSize > 512) {
        printf("error: dns request is larger than allowed max size of 512 bytes");
        return -1;
    }

    Header h;
    memset(&h, 0, sizeof(h));
    uint16_t hBytesRead = parseHeader(&h, buffer);
    printHeader(&h);

    //assuming one question for now
    Question q;
    memset(&q, 0, sizeof(q));
    uint16_t qBytesRead = parseQuestion(&q, &buffer[hBytesRead]); // start at next byte after the header
    printQuestion(&q);

    return 0;
}
#include <assert.h>
#include <string.h>
#include "common.h"
#include "util.h"
#include "types.h"

const uint16_t mask_qr = 1<<15;
const uint16_t mask_opcode = 0x07<<11;
const uint16_t mask_aa = 1<<10;
const uint16_t mask_tc = 1<<9;
const uint16_t mask_rd = 1<<8;
const uint16_t mask_ra = 1<<7;
const uint16_t mask_z = 0x07<<4;
const uint16_t mask_rcode = 0x0f;

uint16_t parseHeader(Header* h, BYTE* rawRequest) {
    BYTE rawHeader[12];
    memcpy(&rawHeader, rawRequest, sizeof(rawHeader));

    h->identifier = rawHeader[0] << 8;
    h->identifier |= rawHeader[1];
    h->flags = rawHeader[2] << 8;
    h->flags |= rawHeader[3];
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

void serializeHeader(BYTE* bytes, Header *h) {
    memcpy(bytes, &(h->identifier), 2);
    memcpy((bytes + 2), &(h->flags), 2);
    memcpy((bytes + 4), &(h->questionCount), 2);
    memcpy((bytes + 6), &(h->answerCount), 2);
    memcpy((bytes + 8), &(h->authorityCount), 2);
    memcpy((bytes + 10), &(h->addtlCount), 2);
}

void printHeader(Header *h) {
    assert(h != NULL);

    printf("HEADER\n");
    printf("\tid: %d\n", h->identifier);
    printf("\tflags: %d\n", h->flags);
    printf("\tqueryResponse: %d\n", GET_BITFLAG(h->flags, mask_qr));
    printf("\topcode: %d\n", GET_OPCODE(h->flags));
    printf("\taa: %d\n", GET_BITFLAG(h->flags, mask_aa));
    printf("\ttc: %d\n", GET_BITFLAG(h->flags, mask_tc));
    printf("\trd: %d\n", GET_BITFLAG(h->flags, mask_rd));
    printf("\tra: %d\n", GET_BITFLAG(h->flags, mask_ra));
    printf("\tz: %d\n", (h->flags & mask_z) >> 4);
    printf("\trcode: %d\n", (h->flags & mask_rcode));
    printf("\tquestionCount: %d\n", h->questionCount);
    printf("\tanswerCount: %d\n", h->answerCount);
    printf("\tauthorityCount: %d\n", h->authorityCount);
    printf("\taddtlCount: %d\n", h->addtlCount);
}

void getRawQuestion(BYTE* rawQuestion, size_t* rawQuestionLen, BYTE* rawRequest) {
    uint16_t bytesRead = 0;
    BYTE* curr = rawRequest; //current pointer

    while (rawRequest[bytesRead] != 0x0) {
        rawQuestion[bytesRead] = rawRequest[bytesRead];
        bytesRead += 1;
    }
    
    rawQuestion[bytesRead] = rawRequest[bytesRead];
    bytesRead += 1;

    //qclass
    memcpy(&(rawQuestion[bytesRead]), &(rawRequest[bytesRead]), 2);
    bytesRead += 2;

    //qtype
    memcpy(&(rawQuestion[bytesRead]), &(rawRequest[bytesRead]), 2);
    bytesRead += 2;

    *rawQuestionLen = bytesRead;
}

int getQName(char *qname, size_t qnameSize, BYTE* rawRequest) {
    if (qnameSize < QNAME_SIZE + 1) {
        printf("qname buffer is not large enough.\n");
        return -1;
    }

    uint16_t bytesRead = 0;
    BYTE* currIn = rawRequest;
    BYTE* currOut = qname;

    char dot = '.';
    while (*currIn != 0) {
        BYTE labelLen = *currIn;
        currIn += 1;
        memcpy(currOut, currIn, labelLen);
        currOut += labelLen;

        memset(currOut, dot, sizeof(dot));
        currOut += sizeof(dot);

        currIn += labelLen;
        bytesRead += (labelLen + 1);
    }

    // add null terminator to qname
    char nullTerm = '\0';
    memset(currOut, nullTerm, sizeof(nullTerm));

    return bytesRead + 1;
}

uint16_t parseQuestion(Question* q, uint16_t qcount, BYTE* rawRequest) {
    uint16_t bytesRead = 0;
    for (int i = 0; i < qcount; i++) {
        BYTE* curr = rawRequest; //current pointer

        // qname
        char qname[QNAME_SIZE + 1];
        int qnameBytes = getQName(qname, sizeof(qname), curr);
        if (qnameBytes <= 0) {
            printf("ERROR: Unable to parse qname.\n");
            printBinStr(qname, sizeof(qname));
            return -1;
        }

        strcpy(q->qname, qname);

        curr += 1;
        bytesRead += qnameBytes + 1; // count the last 0 of the label sequence

        q->qtype |= rawRequest[bytesRead + 1];
        q->qtype |= (rawRequest[bytesRead] << 8);
        curr += 2;
        bytesRead += 2;

        q->qclass = rawRequest[bytesRead + 1];
        q->qclass |= (rawRequest[bytesRead] << 8);
        curr += 2;
        bytesRead += 2;
    }

    return bytesRead;
}

void printQuestion(Question* q) {
    printf("QUESTION\n");
    printf("\tqname: %s\n", q->qname);
    printf("\tqclass: %s\n", stringFromQClass(q->qclass));
    printf("\tqtype: %s\n", stringFromQType(q->qtype));
}

int printDnsRequest(BYTE* buffer, size_t bufferSize) {
    if (bufferSize > MAX_BUFFER) {
        printf("error: dns request is larger than allowed max size of 512 bytes");
        return -1;
    }

    Header h;
    memset(&h, 0, sizeof(h));
    uint16_t hBytesRead = parseHeader(&h, buffer);
    printHeader(&h);

    //assuming one question for now
    Question q[512 / sizeof(Question)];
    memset(&q, 0, sizeof(q));
    uint16_t qBytesRead = parseQuestion(q, h.questionCount, &buffer[hBytesRead]); // start at next byte after the header
    
    for (int i = 0; i < h.questionCount; i++) {
        printQuestion(&q[i]);
    }

    printf("%d bytes read.\n\n", hBytesRead + qBytesRead);
}
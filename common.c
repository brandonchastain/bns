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

    // add null terminator to qname and count it
    char nullTerm = '\0';
    memset(currOut, nullTerm, sizeof(nullTerm));
    bytesRead += 1;

    return bytesRead;
}

uint16_t parseQuestion(Question* q, uint16_t qcount, BYTE* rawRequest) {
    uint16_t bytesRead = 0;
    for (int i = 0; i < qcount; i++) {
        int currentByte = 0;

        // qname
        char qname[QNAME_SIZE + 1];
        int qnameBytes = getQName(qname, sizeof(qname), (rawRequest + bytesRead));
        if (qnameBytes <= 0) {
            printf("ERROR: Unable to parse qname.\n");
            printBinStr(qname, sizeof(qname));
            return -1;
        }

        strcpy(q->qname, qname);

        // count the final 0 at the end of the label sequence
        currentByte += qnameBytes + 1;

        // qtype and qclass
        q->qtype = rawRequest[bytesRead + currentByte];
        q->qtype |= (rawRequest[bytesRead + currentByte + 1] << 8);
        currentByte += 2;

        q->qclass = rawRequest[bytesRead + currentByte];
        q->qclass |= (rawRequest[bytesRead + currentByte + 1] << 8);
        currentByte += 2;

        bytesRead += currentByte;
    }

    return bytesRead;
}

void printQuestion(Question* q) {
    printf("QUESTION\n");
    printf("\tqname: %s\n", q->qname);
    printf("\tqclass: %s\n", stringFromQClass(q->qclass));
    printf("\tqtype: %s\n", stringFromQType(q->qtype));
}

void printResourceRecord(ResourceRecord* rr) {
    printf("RESOURCE RECORD\n");
    printf("\ttype: %d\n", rr->type);
    printf("\tclass: %d\n", rr->class);
    printf("\tname: %s\n", rr->name);
    printf("\tttl: %d\n", rr->ttl);
    printf("\trdlength: %d\n", rr->rdlength);
    printf("\trdata: %d.%d.%d.%d\n", rr->rdata[0], rr->rdata[1], rr->rdata[2], rr->rdata[3]);
}

// returns # bytes written.
size_t serializeResourceRecord(BYTE* bytes, ResourceRecord* rr) {
    size_t offset = 0;
    memcpy((bytes + offset), &(rr->name), strlen(rr->name));
    offset += strlen(rr->name);
    memcpy((bytes + offset), &(rr->type), 2);
    offset += 2;
    memcpy((bytes + offset), &(rr->class), 2);
    offset += 2;
    memcpy((bytes + offset), &(rr->ttl), 4);
    offset += 4;
    memcpy((bytes + offset), &(rr->rdlength), 2);
    offset += 2;

    memcpy((bytes + offset), rr->rdata, rr->rdlength);
    offset += rr->rdlength;

    return offset;
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
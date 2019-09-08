#include <stdint.h>
#include <stdio.h>

#ifndef COMMON_H
#define COMMON_H

#define QNAME_SIZE 255

typedef enum { 
    FALSE,
    TRUE
} BOOLEAN;

typedef unsigned char BYTE;

typedef struct _header {
    uint16_t identifier;
    BYTE queryResponse:1;
    BYTE opCode:4;
    BYTE aa:1;  // authoritative answer
    BYTE tc:1;  // truncated message
    BYTE rd:1;  // recursion desired
    BYTE ra:1;  // recursion available
    BYTE z:3;   // unused. reserved for dnssec
    BYTE rCode:4;
    uint16_t questionCount;
    uint16_t answerCount;
    uint16_t authorityCount;
    uint16_t addtlCount;
} Header;

typedef struct _question {
    char qname[QNAME_SIZE];
    uint16_t qtype; //todo: make enums for these instead of raw bytes
    uint16_t qclass;
} Question;

typedef struct _answer {

}  Answer;

int parseDnsRequest(BYTE*, size_t);

#endif
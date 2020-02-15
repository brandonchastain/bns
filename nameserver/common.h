#include <stdint.h>
#include <stdio.h>
#include "types.h"

#ifndef COMMON_H
#define COMMON_H

void printDnsRequest(BYTE*, size_t);
void printHeader(Header*);
void printQuestion(Question*);

uint16_t parseHeader(Header*, BYTE*);
void printResourceRecord(ResourceRecord*);

void serializeHeader(BYTE*, Header *);
size_t serializeQuestion(BYTE*, Question *);
// returns # bytes written
size_t serializeResourceRecord(BYTE*, ResourceRecord*);

void getRawQuestion(BYTE*, size_t*, BYTE*);
uint16_t parseQuestion(Question*, uint16_t, BYTE*);

int getLabelFromQName(char*, char*);

#endif

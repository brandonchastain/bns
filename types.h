#include <stdint.h>
#include <stdio.h>

#ifndef TYPES_H
#define TYPES_H

#define MAX_BUFFER 512
#define QNAME_SIZE 255

// macros and masks for manipulating header flags
#define GET_BITFLAG(flags, mask) ((flags & mask) > 0)
#define SET_BITFLAG(flags, mask) (flags |= mask)
#define CLEAR_BITFLAG(flags, mask) (flags &= ~mask)
#define GET_OPCODE(flags) ((flags & mask_opcode) >> 11)
#define SET_RCODE(flags, val) (flags |= (val & mask_rcode))

extern const uint16_t mask_qr;
extern const uint16_t mask_opcode;
extern const uint16_t mask_aa;
extern const uint16_t mask_tc;
extern const uint16_t mask_rd;
extern const uint16_t mask_ra;
extern const uint16_t mask_z;
extern const uint16_t mask_rcode;

typedef enum {
    A = 1,
    NS = 2
} QTYPE;

static inline char *stringFromQType(QTYPE qtype) {
    static char *strings[] = { "A", "NS" };
    return strings[qtype - 1];
}

typedef enum {
    IN = 1
} QCLASS;

static inline char *stringFromQClass(QCLASS qclass) {
    static char *strings[] = { "IN" };
    return strings[qclass - 1];
}

typedef enum { 
    FALSE,
    TRUE
} BOOLEAN;

typedef unsigned char BYTE;

typedef struct _header {
    uint16_t identifier;
    uint16_t flags;
    uint16_t questionCount;
    uint16_t answerCount;
    uint16_t authorityCount;
    uint16_t addtlCount;
} Header;

typedef struct _question {
    char qname[QNAME_SIZE];
    QTYPE qtype;
    QCLASS qclass;
} Question;

typedef struct _answer {

}  Answer;


#endif
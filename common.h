#ifndef COMMON_H
#define COMMON_H

typedef enum { 
    FALSE,
    TRUE
} BOOLEAN;

typedef unsigned char BYTE;

typedef struct _rawHeader {
    char headerBytes[12];
} RawHeader;

typedef struct _rawQuestion {
    
} Question;

typedef struct _answer {

}  Answer;

#endif
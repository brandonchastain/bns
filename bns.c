#define _XOPEN_SOURCE

#include <signal.h>
#include <stdio.h>
#include <stdlib.h> 
#include <unistd.h> 
#include <string.h> 
#include <sys/types.h> 
#include <sys/socket.h> 
#include <arpa/inet.h> 
#include <netinet/in.h> 

#include "common.h"
#include "util.h"

#define PORT 50037 
#define MAX_BUFFER_SIZE 1024

static volatile sig_atomic_t ctrlCReceived = 0;
static int sockfd = 0;

void closeAll() {
    close(sockfd);
    printf("Closed the server connection.\n");
}

void ctrlCHandler(int s) {
    ctrlCReceived = 1;
}

void setUpCtrlCHandler() {
    struct sigaction act;
    memset(&act, 0, sizeof(act));
    act.sa_handler = ctrlCHandler;
    sigaction(SIGINT, &act, NULL);
}

void initServerAddr(struct sockaddr_in* serverAddress) {
    memset(serverAddress, 0, sizeof(serverAddress));
    serverAddress->sin_family = AF_INET;
    serverAddress->sin_addr.s_addr = htonl(INADDR_ANY);
    serverAddress->sin_port = htons(PORT);
}

int initSocket() {
    sockfd = socket(AF_INET, SOCK_DGRAM, 0);

    if (sockfd < 0) {
        perror("Socket creation failed.\n");
        exit(EXIT_FAILURE);
    }

    struct sockaddr_in serverAddress;
    initServerAddr(&serverAddress);
    int bindResult = bind(sockfd, (struct sockaddr *)&serverAddress, sizeof(serverAddress));
    if (bindResult < 0) {
        perror("Bind failed.\n");
        exit(EXIT_FAILURE);
    }
}

// returns the RCODE
int answerQuestion(ResourceRecord* ans, Question* q) {
    // if I can handle the qtype...
    if (q->qtype == A) {
        // if I know the answer...
        if (strcmp(q->qname, "www.microsoft.com.") == 0) {
            // respond with the known answer
            memset(ans->name, 0, QNAME_SIZE);
            memcpy(ans->name, q->qname, QNAME_SIZE);
            ans->type = 1; // A type
            ans->class = 1; // IN class
            ans->ttl = 0;
            ans->rdlength = 4;
            ans->rdata[0] = 1;
            ans->rdata[1] = 2;
            ans->rdata[2] = 3;
            ans->rdata[3] = 4;
            ans->size = sizeof(ResourceRecord) + 3;
            return 0;
        }

        return 3; // nxdomain
    }

    return 4; // not implemented
}

void listenForUdpQueries() {
    printf("Listening on port %d\n\n", PORT);

    setUpCtrlCHandler();

    while (!ctrlCReceived) {
        BYTE buffer[MAX_BUFFER_SIZE];
        memset(buffer, 0, MAX_BUFFER_SIZE);

        struct sockaddr_in clientAddr;
        int clientAddrLength = sizeof(clientAddr);
        memset(&clientAddr, 0, clientAddrLength);

        // wait for incoming udp message
        int length = recvfrom(sockfd, buffer, sizeof(buffer) - 1, MSG_WAITALL, (struct sockaddr *)&clientAddr, &clientAddrLength);
        if (length < 0) {
            perror("recvfrom failed");
            break;
        }

        buffer[length] = '\0';
        printf("%d bytes received:\n", length);

        // printf("binary:\n");
        // printBinStr((BYTE*)&buffer, length);
        // printf("hex:\n");
        // printHexStr(buffer, length);

        // Print the DNS request.
        printDnsRequest(buffer, length);

        // Copy the header
        Header h;
        memset(&h, 0, sizeof(h));
        int hByteRead = parseHeader(&h, buffer);

        printf("Creating answer...\n");
        // set header flags
        memset(&(h.flags), 0, sizeof(h.flags));
        SET_BITFLAG(h.flags, mask_qr);
        SET_RCODE(h.flags, 3u); // nxdomain

        Question q;
        int qBytesRead = parseQuestion(&q, 1, &buffer[hByteRead]);

        ResourceRecord rr;
        int responseCode = answerQuestion(&rr, &q);

        if (responseCode == 0) {
            CLEAR_RCODE(h.flags);
            h.answerCount = 1;
            SET_BITFLAG(h.flags, mask_aa);
        }

        printHeader(&h);
        printQuestion(&q);
        if (responseCode == 0) {
            printResourceRecord(&rr);
        }

        // populate response buffer
        BYTE response[MAX_BUFFER];
        memset(response, 0, MAX_BUFFER);

        BYTE sh[sizeof(Header)];
        memset(sh, 0, sizeof(sh));
        Header htemp = h;
        toNetworkOrder(&htemp); // convert multi-byte properties to network order
        serializeHeader(sh, &htemp);

        size_t responseOffset = 0;
        memcpy(response, sh, sizeof(sh)); // copy the serialized header
        responseOffset += sizeof(sh);

        // include the question in the response
        BYTE rawQuestion[512];
        memset(rawQuestion, 0, sizeof(rawQuestion));
        size_t rawQSize = 0;
        getRawQuestion(rawQuestion, &rawQSize, &(buffer[12]));
        memcpy(&(response[responseOffset]), rawQuestion, rawQSize); // copy the questions
        responseOffset += rawQSize;

        BYTE serializedRr[MAX_BUFFER];
        memset(serializedRr, 0, sizeof(MAX_BUFFER));

        if (responseCode == 0) {
            size_t rrBytes = serializeResourceRecord(serializedRr, &rr);
            memcpy(&(response[responseOffset]), serializedRr, rrBytes);
            responseOffset += rrBytes;
        }

        // printf("Response (network order):\n");
        // printHexStr(response, sizeof(sh) + rawQSize);
        int r = sendto(sockfd, response, responseOffset, MSG_CONFIRM, (const struct sockaddr *)&clientAddr, clientAddrLength);
        if (r < 0) {
            perror("sendto failed");
            break;
        }

        printf("answer sent.\n");
    }
}

int main(void) {
    initSocket();
    listenForUdpQueries();
    closeAll();
    return 0;
}

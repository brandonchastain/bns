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
        printf("%d bytes received.\n", length);

        // printf("binary:\n");
        // printBinStr((BYTE*)&buffer, length);
        // printf("hex:\n");
        // printHexStr(buffer, length);

        // Print the DNS request.
        printf("Incoming request:\n");
        printDnsRequest(buffer, length);

        // Copy the header
        Header h;
        memset(&h, 0, sizeof(h));
        int hByteRead = parseHeader(&h, buffer);

        // header flags
        memset(&(h.flags), 0, sizeof(h.flags));
        SET_BITFLAG(h.flags, mask_qr);
        SET_RCODE(h.flags, 3u); // nxdomain

        printf("Creating answer...\n\n");
        printf("Outgoing header:\n");
        printHeader(&h);

        Header htemp = h;
        toNetworkOrder(&htemp); // convert multi-byte properties to network order

        BYTE sh[sizeof(Header)];
        memset(sh, 0, sizeof(sh));
        serializeHeader(sh, &htemp);

        // copy the questions to include in response
        BYTE rawQuestion[512];
        size_t rawQSize = 0;
        getRawQuestion(rawQuestion, &rawQSize, &(buffer[12]));

        // populate response buffer
        BYTE response[MAX_BUFFER];
        memset(response, 0, MAX_BUFFER);
        memcpy(&response, sh, sizeof(sh)); // copy the serialized header
        memcpy(&(response[sizeof(sh)]), rawQuestion, rawQSize); // copy the questions

        // printf("Response (network order):\n");
        // printHexStr(response, sizeof(sh) + rawQSize);

        int r = sendto(sockfd, response, sizeof(sh) + rawQSize, MSG_CONFIRM, (const struct sockaddr *)&clientAddr, clientAddrLength);
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

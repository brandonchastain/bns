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
#define MAX_BUFFER_SIZE 10

static volatile sig_atomic_t ctrlCReceived = 0;
static int sockfd = 0;

void ctrlCHandler(int s) {
    ctrlCReceived = 1;
    close(sockfd);
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
    
    printf("socket created.\n");

    if (sockfd < 0) {
        perror("Socket creation failed.\n");
        exit(EXIT_FAILURE);
    }

    struct sockaddr_in serverAddress;
    initServerAddr(&serverAddress);

    printf("server address initialized.\n");

    int bindResult = bind(sockfd, (struct sockaddr *)&serverAddress, sizeof(serverAddress));
    if (bindResult < 0) {
        perror("Bind failed.\n");
        exit(EXIT_FAILURE);
    }
}

void listenForUdpQueries() {
    printf("Listening on port %d\n", PORT);

    setUpCtrlCHandler();

    while (!ctrlCReceived) {
        BYTE buffer[MAX_BUFFER_SIZE];
        memset(&buffer, 0, MAX_BUFFER_SIZE);

        struct sockaddr_in clientAddr;
        int clientAddrLength = sizeof(clientAddr);
        memset(&clientAddr, 0, clientAddrLength);
        int length = recvfrom(sockfd, buffer, sizeof(buffer) - 1, MSG_WAITALL, (struct sockaddr *)&clientAddr, &clientAddrLength);
        if (length < 0) {
            perror("recvfrom failed");
            break;
        }

        buffer[length] = '\0';

        printf("%d bytes: '%s'\n", length, buffer);
        printf("binary [%d]:\n", MAX_BUFFER_SIZE);
        printBinStr((BYTE*)&buffer, MAX_BUFFER_SIZE);

        const char* ack = "acking udp request.";
        sendto(sockfd, ack, strlen(ack), MSG_CONFIRM, (const struct sockaddr *)&clientAddr, clientAddrLength);
        printf("ack message sent.\n");
    }
}

int main(void) {
    printf("beginning init\n");
    initSocket();
    printf("initialized socket\n");

    listenForUdpQueries();
    
    close(sockfd);
    printf("Closed the server connection.\n");

    return 0;
}

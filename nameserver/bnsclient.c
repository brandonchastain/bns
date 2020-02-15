#include <stdio.h> 
#include <stdlib.h> 
#include <unistd.h> 
#include <string.h> 
#include <sys/types.h> 
#include <sys/socket.h> 
#include <arpa/inet.h> 
#include <netinet/in.h> 
#include <netdb.h> 

#include "types.h"
#include "common.h"
  
#define PORT 50037

void initServerAddr(struct sockaddr_in* serveraddr) {
    memset(serveraddr, 0, sizeof(struct sockaddr_in));
    serveraddr->sin_family = AF_INET;
    serveraddr->sin_port = htons(PORT);
    serveraddr->sin_addr.s_addr = htonl(0x7f000001);
}

void createRequest(DnsRequest* request) {
    memset(&request, 0, sizeof(request));
    
    SET_BITFLAG(request->header.flags, mask_rd); // recursion desired
    request->header.identifier = 0xffff;
    request->header.questionCount = 1;

    request->question.qclass = IN;
    request->question.qtype = A;

    char* qname = "www.microsoft.com";
    strncpy(request->question.qname, qname, strlen(qname));
}

int main(int argc, char **argv) {

    int sockfd;

    if ((sockfd = socket(AF_INET, SOCK_DGRAM, 0)) < 0) {
        perror("socket failed");
        return 1;
    }

    struct sockaddr_in serveraddr;
    initServerAddr(&serveraddr);

    DnsRequest request;
    createRequest(&request);
    BYTE *serializedRequest = serializeRequest(&request);


    if (sendto(sockfd, "hello", 5, MSG_CONFIRM, (struct sockaddr *)&serveraddr, sizeof(serveraddr)) < 0) {
        perror("sendto failed");
    }
    
    printf("query sent\n");

    BYTE buffer[MAX_BUFFER];
    memset(buffer, 0, sizeof(buffer));
    int n, len;
    n = recvfrom(sockfd, &buffer, sizeof(buffer), MSG_WAITALL, (struct sockaddr *)&serveraddr, &len);
    if (n < 0) {
        perror("recvfrom failed");
    }

    close(sockfd);
}
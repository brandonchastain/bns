#include <stdio.h> 
#include <stdlib.h> 
#include <unistd.h> 
#include <string.h> 
#include <sys/types.h> 
#include <sys/socket.h> 
#include <arpa/inet.h> 
#include <netinet/in.h> 
#include <netdb.h> 

#include "common.h"
  
#define PORT 50037
#define MAX_BUFFER_SIZE 10

int main(void) {
    int sockfd;

    if ((sockfd = socket(AF_INET, SOCK_DGRAM, 0)) < 0) {
        perror("socket failed");
        return 1;
    }

    struct sockaddr_in serveraddr;
    memset(&serveraddr, 0, sizeof(serveraddr));
    serveraddr.sin_family = AF_INET;
    serveraddr.sin_port = htons(PORT);
    serveraddr.sin_addr.s_addr = htonl(0x7f000001);

    for (int i = 0; i < 4; i++) {
        if (sendto(sockfd, "hello", 5, MSG_CONFIRM, (struct sockaddr *)&serveraddr, sizeof(serveraddr)) < 0) {
            perror("sendto failed");
            break;
        }
        
        printf("message sent\n");

        BYTE buffer[MAX_BUFFER_SIZE];
        memset(&buffer, 0, sizeof(buffer));
        int n, len;
        n = recvfrom(sockfd, &buffer, sizeof(buffer), MSG_WAITALL, (struct sockaddr *)&serveraddr, &len);
        if (n < 0) {
            perror("recvfrom failed");
            break;
        }
        
        buffer[n] = '\0';
        printf("Server: %s\n", buffer);
    }

    close(sockfd);
}
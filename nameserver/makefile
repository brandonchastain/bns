CC=gcc
CFLAGS=-Wall -g -I.
DEPS = types.h util.h common.h
OBJ = bns.o util.o common.o
CLIENTOBJ = bnsclient.o util.o common.o
TESTOBJ = test.o util.o common.o

%.o: %.c $(DEPS)
	$(CC) -c -o $@ $< $(CFLAGS)

bns: $(OBJ)
	$(CC) -o $@ $^ $(CFLAGS)

bnsclient: $(CLIENTOBJ)
	$(CC) -o $@ $^ $(CFLAGS)

test: $(TESTOBJ)
	$(CC) -o $@ $^ $(CFLAGS)

clean:
	rm -f *.o
	rm -f *.out
	rm -f bnsclient
	rm -f bns
	rm -f test

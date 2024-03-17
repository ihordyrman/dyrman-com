---
date: 2024-03-17 21:22:20
page-title: Networking Models
url: article-2
tags: networking, osi
---
The OSI seven-layer model encourages modular design in networking, meaning that each layer has as little to do with the operation of other layers as possible.
Each layer on the model trusts that the other layers on the model do their jobs.
- **Layer 7** Application
- **Layer 6** Presentation
- **Layer 5** Session
- **Layer 4** Transport
- **Layer 3** Network
- **Layer 2** Data Link
- **Layer 1** Physical
Please Do Not Throw Sauce Pizza Away

The OSI seven layers are not laws of physics—anybody who wants to design a network can do it any way he or she wants. Although many protocols fit neatly into one of the seven layers, others do not.
Networking professionals use models to conceptualize the many parts of a network, relying primarily on the Open Systems Interconnection (OSI) seven-layer model.
Why OSI Model is helpful - mental tool for diagnosing problems.

#### Layer 1
Layer 1 of the OSI model defines the method of moving data between computers.
Anything that moves data from one system to another, such as copper cabling, fiber optics, even radio waves, is part of the OSI Physical layer.
Layer 1 doesn’t care what data goes through; it just moves the data from one system to another system.
![[Pasted image 20240302163730.png]]
The network interface card, or NIC (pronounced “nick”), which serves as the interface between the PC and the network.

Inside every NIC, burned onto some type of ROM chip, is special firmware containing a unique identifier with a 48-bit value called the media access control address, or MAC address.
MAC addresses are always written in hex.
If MAC address is 00–40–05–60–7D–49:
- The first six digits, in this example 00–40–05, represent the number of the NIC manufacturer.
- The last six digits, in this example 60–7D–49, are the manufacturer’s unique serial number.

Think of a charge (or light) on the wire as a one and no charge (or light) as a zero.

How does the network get the right data to the right system? All networks transmit data by breaking whatever is moving across the Physical layer (such as files, print jobs, Web pages, and so forth) into discrete chunks called frames. A frame is basically a container for a chunk of data moving across a network. A frame encapsulates—puts a wrapper around— information and data for easier transmission.

The frame begins with the MAC address of the NIC to which the data is to be sent, followed by the MAC address of the sending NIC. Next comes the Type field, which indicates what’s encapsulated in the frame. Then comes the Data field that contains what’s encapsulated, followed by a special piece of checking information called the frame check sequence (FCS). The FCS uses a type of binary math called a cyclic redundancy check (CRC) that the receiving NIC uses to verify that the data arrived intact.
![[Pasted image 20240302165345.png]]
The frames used in Ethernet networks hold at most 1500 bytes of data.
As the receiving system begins to accept the incoming frames, the receiving system’s software recombines the data chunks as they come in from the network.

The primary purpose of the FCS (Frame Check Sequence) is to detect errors in the transmitted frame. As data travels over a network, it can be affected by noise, interference, or other issues causing bits to flip (changing 0s to 1s or vice versa).
The FCS helps identify if such an error has occurred.
When a frame is being prepared for transmission, the sender computes the FCS based on the frame's contents, usually using a mathematical algorithm like a Cyclic Redundancy Check (CRC).
TCP keeps track of data packets sent and receives acknowledgments (ACKs) from the receiver. If a packet (frame) is dropped and not acknowledged, TCP will retransmit it after a timeout period.

How does a sending NIC know the MAC address of the NIC to which it’s sending the data?
In most cases, the sending system already knows the destination MAC address because the NICs had probably communicated earlier, and each system stores that data.
Without knowing the MAC address to begin with, the requesting computer will use an IP address to pick the target computer out of the crowd.

#### Layer 2
Any device that deals with a MAC address is part of the OSI Data Link layer, or Layer 2 of the OSI model.

Sending NICs break frames down into ones and zeroes for transmission; receiving NICS rebuild the frame on receipt.
The LLC (Logical Link Control) is the aspect of the NIC that talks to the system’s operating system (usually via device drivers). The LLC handles multiple network protocols and provides flow control.

Media Access Control (MAC), which creates and addresses the frame. It adds the NIC’s own MAC address and attaches MAC addresses to the frames.
Recall that each frame the NIC creates must include both the sender’s and recipient’s MAC addresses. The MAC sublayer adds or checks the FCS.
The MAC also ensures that the frames, now complete with their MAC addresses, are then sent along the network cabling.
NICs, therefore, operate at both Layer 2 and Layer 1 of the OSI seven-layer model.

#### Layer 3
Large networks need a logical addressing method, like a postal code or telephone numbering scheme, that ignores the hardware and enables you to break up the entire large network into smaller networks called subnets
To move past the physical MAC addresses and start using logical addressing requires some special software called a network protocol. Network protocols exist in every operating system. A network protocol not only has to create unique identifiers for each system, but also must create a set of communication rules for issues like how to handle data chopped up into multiple packets and how to ensure those packets get from one subnet to another.
To be accurate, TCP/IP is really several network protocols designed to work together— better known as a protocol suite—but two protocols, TCP and IP, do so much work that the folks who invented all these protocols named the whole thing TCP/IP.

At the Network layer, Layer 3, containers called packets get created and addressed so they can go from one network to another.

An IP address is known as a logical address to distinguish it from the physical address, the MAC address of the NIC.

For a TCP/IP network to send data successfully, the data must be wrapped up in two distinct containers.
A frame of some type enables the data to move from one device to another.
Inside that frame are both an IP-specific container that enables routers to determine where to send data—regardless of the physical connection type—and the data itself. In TCP/IP, that inner container is the packet.
![[Pasted image 20240302175652.png]]

Each IP packet is handed to the NIC, which then encloses the IP packet in a regular frame, creating, in essence, a packet within a frame.
![[Pasted image 20240303161256.png]]
Each router strips off the incoming frame, determines where to send the data according to the IP address in the packet, creates a new frame, and then sends the packet within a frame on its merry way. The new frame type will be the appropriate technology for whatever connection technology connects to the next router. The IP packet, on the other hand, remains unchanged.

Once the packet reaches the destination subnet’s router, that router strips off the incoming frame — no matter what type — looks at the destination IP address, and then adds a frame with the appropriate destination MAC address that matches the destination IP address.

#### Layer 4
Because most chunks of data are much larger than a single packet, they must be chopped up before they can be sent across a network. When a serving computer receives a request for some data, it must be able to chop the requested data into chunks that will fit into a packet (and eventually into the NIC’s frame), organize the packets for the benefit of the receiving system, and hand them to the NIC for sending. This is called segmentation.

The receiving system does the reassembly of the packets. It must recognize a series of incoming packets as one data transmission, reassemble the packets correctly based on information included in the packets by the sending system, and verify that all the packets for that piece of data arrived in good shape.
The transport protocol breaks up the data into chunks called segments and gives each segment some type of sequence number.

Layer 4, the Transport layer of the OSI seven-layer model, has a big job: it’s the segmentation/reassembly software. As part of its job, the Transport layer also initializes requests for packets that weren’t received in good order.

To see the Transport layer in action, strip away the IP addresses from an IP packet. What’s left is a chunk of data in yet another container called a TCP segment. TCP segments have many other fields that ensure the data gets to its destination in good order. These fields have names such as Source port, Destination port, Sequence number, and Acknowledgment number.
![[Pasted image 20240303163642.png]]
Many TCP segments come into any computer. The computer needs some way to determine which TCP segments go to which applications. A Web server, for example, sees a lot of traffic, but it “listens” or looks for TCP segments with the destination port numbers 80 or 443, grabs those segments, and processes them. Equally, every TCP segment contains another port number — the source port — so the client knows what to do with returning information.

Data comes from the Application layer (with perhaps some input from Presentation and Session). The Transport layer breaks that data into chunks, adding port numbers and sequence numbers, creating the TCP segment. The Transport layer then hands the TCP segment to the Network layer, which, in turn, creates the IP packet.

Following the same process, the Transport layer adds port and length numbers plus a checksum as a header and combines with data to create a container called a UDP datagram. A UDP datagram lacks most of the extra fields found in TCP segments, simply because UDP doesn’t care if the receiving computer gets its data.
![[Pasted image 20240303164116.png]]

### Layer 5
The session software connects applications to applications.
Layer 5, the Session layer of the OSI seven-layer model, handles all the sessions for a system. The Session layer initiates sessions, accepts incoming sessions, and opens and closes existing sessions.

### Layer 6
The Presentation layer translates data from lower layers into a format usable by the Application layer, and vice versa.
This manifests in several ways and isn’t necessarily clear-cut. The messiness comes into play because TCP/IP networks don’t necessarily map directly to the OSI model.
A number of protocols function on more than one OSI layer and can include Layer 6, Presentation. The encryption protocol used in e-commerce, Transport Layer Security (TLS), for example, seems to initiate at Layer 5 and then encrypt and decrypt at Layer 6. But even one of the authors of the protocol disputes that it should even be included in any OSI chart! It makes for some confusion.

#### Layer 7
The Application layer is Layer 7 in the OSI seven-layer model. Keep in mind that the Application layer doesn’t refer to the applications themselves. It refers to the code built into all operating systems that enables network-aware applications. All operating systems have Application Programming Interfaces (APIs) that programmers can use to make their programs network aware.
An API, in general, provides a standard way for programmers to enhance or extend an application’s capabilities.
![[Pasted image 20240303191636.png]]

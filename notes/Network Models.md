---
date: 2024-03-17 21:22:20
title: Networking Models
path: article-2
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

#### Layer 1
Layer 1 of the `OSI model` defines the method of moving data between computers.
Anything that moves data from one system to another, such as copper cabling, fiber optics, even radio waves, is part of the **OSI Physical layer**.
Layer 1 doesn’t care what data goes through; it just moves the data from one system to another system.
![[20240302163730.png]]
The network interface card, or NIC (pronounced “nick”), which serves as the interface between the PC and the network.

Inside every NIC, burned onto some type of ROM chip, is special firmware containing a unique identifier with a 48-bit value called the media access control address, or MAC address.
MAC addresses are always written in hex.
If MAC address is 00–40–05–60–7D–49:
- The first six digits, in this example 00–40–05, represent the number of the NIC manufacturer.
- The last six digits, in this example 60–7D–49, are the manufacturer’s unique serial number.

Please read this: [Unit 1: OSI Model](https://bhg2.wordpress.com/wp-content/uploads/2017/11/unit1cn.pdf)

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder();
var currentAssembly = Assembly.GetExecutingAssembly();

builder.Host.AddSerilog()
    .AddLocalization<IErrorMessagesLocalizer, IEntitiesLocalizer, IEnumsLocalizer, IReportColumnsLocalizer>(
        new AssemblyName(typeof(PaymentSystem).GetTypeInfo().Assembly.FullName!).Name!)
    .AddMasterDbContext<AdminCabinetMasterDbContext, MasterDbContext>()
    .AddReplicaDbContext<ReplicaDbContext>()
    .AddOpenIddict()
    .AddOpenTelemetry(builder.Environment.ApplicationName)
    .AddKafka<Program>();
```

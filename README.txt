#Responsr Integration template project

------------------------------------------------------------------------------------
# Version 1.02
------------------------------------------------------------------------------------

This template is created to make it very simple to integrate any application to Responsr.

The sample application has some mockup code to show how this could work if you would 
send bookings from a database, but the template works as well with API calls.

It is created as a Console Application that can be triggered by a Scheduled Task.

------------------------------------------------------------------------------------
# Context
------------------------------------------------------------------------------------

Responsr is a customer loyalty software. To work it needs customer transactions sent
to it. It is created to work in a SaaS (Software as a Service) environment.

Your clients have a user account in Responsr or they sign up to Responsr to get one. 
When they do they will recieve an "app", a ready to use customer loyalty module 
linked to your system.

Within this app they can enter a "Client id" or an "Integration key" towards your system. 
This is how we both keep track on who of your clients uses Responsr and who you will 
send transactions for. 

* We will provide you with a list of the active clients
* We will provide you with information on your latest call, so you don't send the 
  same data over and over again.
* We will make sure the end customers are not spammed by information from Responsr

------------------------------------------------------------------------------------
First step
------------------------------------------------------------------------------------

Download the Nugetpackage. Add a new Nuget source in Visual Studio.
The source URL is 
https://qsdev.pkgs.visualstudio.com/ResponsrPublic/_packaging/ResponsrPartner/nuget/v3/index.json

Install the following packages:
*  Responsr.Partner.Integration
*  Microsoft.Extensions.Logging.Abstractions
*  Newtonsoft.Json

------------------------------------------------------------------------------------
# Key concepts
------------------------------------------------------------------------------------

PartnerKey:		
You recieve an integration key from us. Enter it into app.config
This key is used by the application to list who of your clients are active and when
sending data to us. The template application will do this automatically for you.

ClientId:		
A unique, not easily guessed, key that we both share to identify your different clients. 

latestRetrievalTimestamp:
The last time you sent data to Responsr. You only need to send newer transactions.

When to send:
You send data when something is finished, such as when someone checked out of a hotel,
when they left the restaurant or when the customer service ticket is closed.

------------------------------------------------------------------------------------
# What data and formats
------------------------------------------------------------------------------------

Each interaction is centered around a Person. This is the end Customer.
Based on your application there are attributes that are relevant to send to us such
as names, email, phone numbers etc.

A person that interacted with a end customer is called an Agent. This can be a 
sales representative or customer service employee.

All attributes sent to Responsr are handled as string.

The sample application has the following attributes:
* firstname
* lastname
* email
* price
* pricePlan
* campaignName
* room
* checkin
* checkout
* bookingTime
* bookingReference

Examples from Customer Service integration:
* mobilePhone
* resolutionTime
* createdAt
* referenceId
* agentId
* agentName
* agentEmail
* tags

------------------------------------------------------------------------------------
# Debuging and logging
------------------------------------------------------------------------------------

The application will automatically log some events and there is a structure for easy
debugging when running the application.

LogFolderPath:	
This is where the application stores the logs. This folder must exist or the 
application will fail.

Your own logs:
Ofcourse you'll want to log some interactions to your system as well. 
You can do that through:
_logger.LogInformation("YOUR MESSAGE HERE");

Debugging:
The application has a debug setting in application settings, but will also react to
tries to debugging it such as adding "help", "debug", "verbose" as arguments on
the command line. 

Your own Debug comments
You can do this through
Debug("YOUR MESSAGE HERE");


------------------------------------------------------------------------------------
# VERSION HISTORY
------------------------------------------------------------------------------------
1.0	Initial version
1.01	Added README, log and debug functionality
1.02	Updated for v2 of nuget package

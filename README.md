# ing-ideal-connectors-net
Opensource tools and API to connect webshops and merchants to ING using iDeal


testing requires installed certificates and the tester needs acces rights for the private key part of the certificate

### Konfidence.ing.iDealAdvancedConnector package is available on [nuget.org](https://www.nuget.org/packages/Konfidence.ing.iDealAdvancedConnector). 

breaking change:

Connector connector = New Connector(); => IConnector connector = Connector.CreateConnector();
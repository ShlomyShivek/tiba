Todos:
1. change console to real log file
2. implement real authorization service
3. implement unit tests
4. rabbitmq (and any other parameter) should be configurable

6. make sure rabbit persist the data
7. consider moving the "shared" library to a different repo and consume it as nugget package (in case of other projects depending on it)
8. having the same model for rabbit and for rest api is wrong. we should create a separate model in the rest service and use mapper to map between them.
9. review the rabbt queue parameters
10. handle build warnings
11. namespaces with capital letters please
12. add healthcheck to rest service
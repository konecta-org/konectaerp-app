FROM maven:3.9.9-eclipse-temurin-17 AS build
WORKDIR /workspace
COPY pom.xml .
COPY src ./src
RUN mvn -B clean package -DskipTests
# ARG MAVEN_CLI_OPTS="-Dmaven.wagon.http.retryHandler.count=5 -Dmaven.wagon.http.retryHandler.class=standard -Dmaven.wagon.httpconnectionManager.ttlSeconds=60"
# RUN mvn -B ${MAVEN_CLI_OPTS} -DskipTests dependency:go-offline
# RUN mvn -B ${MAVEN_CLI_OPTS} clean package -DskipTests

FROM eclipse-temurin:17-jre
WORKDIR /app
ENV JAVA_OPTS=""
COPY --from=build /workspace/target/ReportingService-0.0.1-SNAPSHOT.jar app.jar
EXPOSE 8085
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar app.jar"]

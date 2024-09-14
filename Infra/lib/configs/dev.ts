import { StringParameter } from 'aws-cdk-lib/aws-ssm'
import { Construct } from 'constructs'
import { InfraStackProps } from '../infra-stack'


const props = {
  env: { account: '030464729230', region: 'eu-central-1' },
    imageTag: 'main-1ba961b',
    sslCertParameterName: 'WILDCARD_SSL_CERT',
  vpcTags: {
    Name: 'ConnectivityAgentDevStack/ConnectivityAgentVpc',
    'aws:cloudformation:stack-name': 'ConnectivityAgentDevStack',
  },
  tags: {
    environment: 'dev',
    product: 'workflow-configurator',
    service: 'workflow-configurator',
  },
  mongoDB: {
    projectId: '62bac43d62c1ff433fe53371',
    clusterName: 'DedicatedDevCluster',
    clusterURL: 'mongodb+srv://dedicateddevcluster.0yddt.mongodb.net',
    profile: 'dev',
    databaseName: 'WorkflowConfiguration',
  },
}

export const devConfig = (): InfraStackProps => ({
  ...props,
  containerEnvironment: {
    workflowConfiguratorEnv: {
      MaterialAddress: 'https://configurator.dev.arxum.app/Material',
      ConnectionStrings__DBConnection: props.mongoDB.clusterURL,
          CorsPolicyUrl: 'https://*.arxum.app',
          ASPNETCORE_ENVIRONMENT: "Development",
    },
  },
})


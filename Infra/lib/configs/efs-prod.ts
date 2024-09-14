import { InfraStackProps } from '../infra-stack'
import { Repository } from 'aws-cdk-lib/aws-ecr'

const props = {
  env: { account: '457621392112', region: 'eu-central-1' },
    imageTag: 'main-1348918', // image tag to be deployed, if new, update srcImageTag to be copied
  sslCertParameterName: 'WILDCARD_PROD_SSL_CERT',
  vpcTags: {
    Name: 'ConnectivityAgentProdStack/SharedVpc',
    'aws:cloudformation:stack-name': 'ConnectivityAgentProdStack',
  },
  tags: {
    environment: 'prod',
    product: 'workflow-configurator',
    service: 'workflow-configurator',
  },
  mongoDB: {
    projectId: '646cc9869e35d87d028061ce',
    clusterName: 'DedicatedProdCluster',
    clusterURL: 'mongodb+srv://dedicatedprodcluster-pl-0.sx2vr.mongodb.net/WorkflowConfiguration?authSource=%24external&authMechanism=MONGODB-AWS&retryWrites=true&w=majority',
    profile: 'prod',
    databaseName: 'WorkflowConfiguration',
  },
  apiKeyPrefixName: 'IONOS_API_PREFIX',
  apiKeyEncryptionName: 'IONOS_API_ENCRYPTION',
  zoneId: '53bf2bb3-0877-11eb-952c-0a5864442b21',
  ionosApi: 'https://api.hosting.ionos.com/dns',
}

export const prodConfig = (srcRepository: Repository): InfraStackProps => ({
  ...props,
  srcEcrRepository: {
    srcRepository,
    //TODO this is staging ECR
    srcAccount: '487727660817',
    srcRepoName:
      'workflowconfiguratorecrstagingstack-workflowconfiguratorecrb5f784e4-fwm6vzjeyhjd',
      srcImageTag: props.imageTag, // image tag to push to target ECR
  },
  containerEnvironment: {
    workflowConfiguratorEnv: {
      MaterialAddress: 'https://configurator.prod.arxum.app/Material',
      ConnectionStrings__DBConnection: props.mongoDB.clusterURL,
          CorsPolicyUrl: 'https://*.arxum.app',
          ASPNETCORE_ENVIRONMENT: "Production",
    },
  },
})

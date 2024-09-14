import { InfraStackProps } from '../infra-stack'
import { Repository } from 'aws-cdk-lib/aws-ecr'

const props = {
  env: { account: '487727660817', region: 'eu-central-1' },
    imageTag: 'main-299211b', // image tag to be deployed, if new, update srcImageTag to be copied
  sslCertParameterName: 'WILDCARD_STAGING_SSL_CERT',
  vpcTags: {
    Name: 'ConnectivityAgentStack/ConnectivityAgentVpc',
    'aws:cloudformation:stack-name': 'ConnectivityAgentStack',
  },
  tags: {
    environment: 'staging',
    product: 'workflow-configurator',
    service: 'workflow-configurator',
  },
  mongoDB: {
    projectId: '6388b85ed6472c1d6d78c054',
    clusterName: 'DedicatedStagingCluster',
    clusterURL: 'mongodb+srv://dedicatedstagingcluster.36bww.mongodb.net',
    profile: 'staging',
    databaseName: 'WorkflowConfiguration',
  },
}

export const stagingConfig = (srcRepository: Repository): InfraStackProps => ({
  ...props,
  srcEcrRepository: {
    srcRepository,
    srcAccount: '030464729230',
    srcRepoName:
      'workflowconfiguratorecrdevstack-workflowconfiguratorecrb5f784e4-pbfmlloibtnq',
      srcImageTag: 'main-299211b', // image tag to push to target ECR
  },
  containerEnvironment: {
    workflowConfiguratorEnv: {
      MaterialAddress: 'https://configurator.test.arxum.app/Material',
      ConnectionStrings__DBConnection: props.mongoDB.clusterURL,
          CorsPolicyUrl: 'https://*.arxum.app',
          ASPNETCORE_ENVIRONMENT: "Testing",
    },
  },
})

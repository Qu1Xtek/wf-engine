import { Construct } from 'constructs'
import { WorkflowConfiguratorConstruct, srcEcrRepository } from './workflow-construct'
import { SecurityGroup, Vpc } from 'aws-cdk-lib/aws-ec2'
import { Cluster, ContainerDefinitionOptions } from 'aws-cdk-lib/aws-ecs'
import {
  ApplicationLoadBalancer,
  ApplicationProtocol,
  ListenerAction,
} from 'aws-cdk-lib/aws-elasticloadbalancingv2'
import { CfnOutput, Duration, Stack, StackProps } from 'aws-cdk-lib'
import { Certificate } from 'aws-cdk-lib/aws-certificatemanager'
import { StringParameter } from 'aws-cdk-lib/aws-ssm'
import { Repository } from 'aws-cdk-lib/aws-ecr'
import { MongoDBAtlasConstruct } from './mongodb-atlas-construct'
export interface InfraStackProps extends StackProps {
  readonly vpcTags?: { [key: string]: string }
    repository?: Repository
    srcEcrRepository?: srcEcrRepository;
  imageTag: string
  sslCertParameterName: string
  tags: {
    environment: string
    product: string
  }
  mongoDB: {
    projectId: string
    clusterName: string
    clusterURL: string
    databaseName: string
    profile: string
  }

  containerEnvironment?: {
    [key: string]: ContainerDefinitionOptions['environment']
  }
  apiKeyPrefixName?: string,
  apiKeyEncryptionName?: string,
  zoneId?: string,
  ionosApi?: string,
  endpointSGParamName? : string
}

export class InfraStack extends Stack {
  constructor(scope: Construct, id: string, props: InfraStackProps) {
    super(scope, id, props)

    const {
      vpcTags,
      tags,
      mongoDB,
      srcEcrRepository,
      repository,
      imageTag,
      containerEnvironment,
      sslCertParameterName,
      apiKeyPrefixName,
      apiKeyEncryptionName,
      zoneId,
      ionosApi,
      endpointSGParamName
    } = props

    const vpc = Vpc.fromLookup(this, 'VPC', {
      tags: vpcTags,
    })

    const cluster = new Cluster(this, 'WorkflowConfiguratorCluster', {
      vpc,
      containerInsights: true,
    })

    cluster.addDefaultCloudMapNamespace({ name: 'workflow-configurator.local' })

    const loadBalancer = new ApplicationLoadBalancer(
      this,
      'WorkflowConfiguratorALB',
      {
        vpc,
        internetFacing: true,
        idleTimeout: Duration.seconds(120),
      },
    )

    const sslCertARN = StringParameter.valueForStringParameter(
      this,
      sslCertParameterName,
      )

    const httpsListener = loadBalancer.addListener(
      'WorkflowConfiguratorALBListenerHTTPS',
      {
        protocol: ApplicationProtocol.HTTPS,
        port: 443,
        certificates: [
          Certificate.fromCertificateArn(this, 'Certificate', sslCertARN),
        ],
        // defaultAction: ListenerAction.fixedResponse(404, {
        // messageBody: 'Not found',
        // }),
      },
    )

    const httpListener =
      (tags.environment === 'dev' &&
        loadBalancer.addListener('WorkflowConfiguratorALBListenerHTTP', {
          port: 80,
        })) ||
      undefined

    new WorkflowConfiguratorConstruct(this, 'WorkflowConfiguratorConstruct', {
      cluster,
      appListeners: {
        httpsListener,
        httpListener,
      },
      environment: containerEnvironment?.workflowConfiguratorEnv,
      imageTag,
        repository: repository!,
        srcEcrRepository,
      mongoDB,
      tags,
      endpointSGParamName 
    })
<<<<<<< Updated upstream

    //Use the code below to create a DNS record in Ionos.
    // const result = new IonosDnsCustomResource(
    //   this,
    //   `WorkflowConfiguratorIonosDnsCustomResource`,
    //   {
    //     target: loadBalancer.loadBalancerDnsName,
    //     hostName: `configurator.${tags.environment}.arxum.app`,
    //     apiKeyPrefix: apiKeyPrefixName as string,
    //     apiKeyEncryption: apiKeyEncryptionName as string,
    //     zoneId: zoneId,
    //     ionosApi: ionosApi,
    //     account: this.account,
    //     uniqueId: `WorkflowConfigurator${`${tags.environment
    //       .slice(0, 1)
    //       .toUpperCase()}${tags.environment.slice(1)}`}IonosRecord`,
    //   },
    // )

    // new CfnOutput(this, 'IonosDnsCustomResource', {
    //   value: result.result,
    // })
=======
>>>>>>> Stashed changes
  }
}

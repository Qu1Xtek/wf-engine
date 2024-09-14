import { Construct } from 'constructs'
import {
  AwsLogDriver,
  ContainerImage,
  FargateService,
  FargateTaskDefinition,
  ICluster,
  PropagatedTagSource,
} from 'aws-cdk-lib/aws-ecs'
import { Repository } from 'aws-cdk-lib/aws-ecr'
import { DnsRecordType } from 'aws-cdk-lib/aws-servicediscovery'
import { ApplicationListener } from 'aws-cdk-lib/aws-elasticloadbalancingv2'
import { MongoDBAtlasConstruct } from './mongodb-atlas-construct'
import { Port, SecurityGroup } from 'aws-cdk-lib/aws-ec2'
import * as ecrdeploy from 'cdk-ecr-deployment'
import { AccountPrincipal } from 'aws-cdk-lib/aws-iam'

export interface srcEcrRepository {
  srcRepository: Repository
  srcAccount: string
  srcRepoName: string
  srcImageTag: string
}
import { secrets } from './configs/secrets'
import { StringParameter } from 'aws-cdk-lib/aws-ssm'
import { version } from 'os'

interface WorkflowConfiguratorConstructProps {
  readonly repository: Repository
  readonly imageTag: string
  readonly environment?: { [key: string]: string }
  readonly cluster: ICluster
  readonly appListeners: {
    httpsListener?: ApplicationListener
    httpListener?: ApplicationListener
  }
  mongoDB: {
    projectId: string
    clusterName: string
    clusterURL: string
    profile: string
    databaseName: string
  }
  tags: {
    environment: string
    product: string
  }
  srcEcrRepository?: srcEcrRepository
  endpointSGParamName? : string
}

export class WorkflowConfiguratorConstruct extends Construct {
  constructor(
    scope: Construct,
    id: string,
    props: WorkflowConfiguratorConstructProps,
  ) {
    super(scope, id)

    const {
      repository,
      srcEcrRepository,
      imageTag,
      environment,
      cluster,
      appListeners: { httpsListener, httpListener },
      mongoDB,
      tags,
      endpointSGParamName
    } = props

    const { srcRepository, srcAccount, srcRepoName, srcImageTag } =
      srcEcrRepository ?? {}

    // TODO grant ECR access to copy-Lambda function only
    // Doing it here requires use of name: PhysicalName.GENERATE_IF_NEEDED in ECR Stack
    //  or constructing a policy here or in ECR stack
    srcRepository?.grantPull(new AccountPrincipal(cluster.stack.account))

    srcEcrRepository &&
      new ecrdeploy.ECRDeployment(this, 'CopyImageBetweenECRs', {
        src: new ecrdeploy.DockerImageName(
          `${srcAccount}.dkr.ecr.${cluster.stack.region}.amazonaws.com/${srcRepoName}:${srcImageTag}`,
        ),
        // TODO implement dstImageTag if it should diff from srcImageTag?!
        dest: new ecrdeploy.DockerImageName(
          `${repository.repositoryUri}:${srcImageTag}`,
        ),
      })

    const taskDefinition = new FargateTaskDefinition(
      this,
      'WorkflowConfiguratorTaskDef',
      {},
    )

    const dipUser = StringParameter.valueForStringParameter(
      scope,
      'DIP_ORG_USER',
      1
      )

      const dipUrl = StringParameter.valueForStringParameter(
          scope,
          'DIP_URL',
          1
      )

    const container = taskDefinition.addContainer('workflow-configurator', {
      image: ContainerImage.fromEcrRepository(
        // Repository.fromRepositoryName(
        //   this,
        //   'WorkflowConfiguratorRepo',
        //   repositoryName,
        // ),
        repository,
        imageTag,
      ),
      environment: {
        ...environment,
        DIP_USER: dipUser,
        DIP_URL: dipUrl
      },
      secrets: secrets(this),
      logging: AwsLogDriver.awsLogs({ streamPrefix: 'workflow-configurator' }),
    })

    container.addPortMappings({ containerPort: 80 })

    const service = new FargateService(this, 'WorkflowConfiguratorService', {
      cluster,
      taskDefinition,
      enableECSManagedTags: true,
      propagateTags: PropagatedTagSource.SERVICE,
      cloudMapOptions: { dnsRecordType: DnsRecordType.A },
      enableExecuteCommand: true,
      assignPublicIp: true,
    })

    const endpointSG =
      endpointSGParamName &&
      StringParameter.valueFromLookup(this, endpointSGParamName)

    const serviceSecurityGroup = service.connections.securityGroups[0]
    const interfaceEndpointSG =
      endpointSGParamName &&
      SecurityGroup.fromLookupById(
        this,
        'VpcInterfaceEndpointSG',
        endpointSG?.valueOf() as string,
      )

    interfaceEndpointSG &&
      interfaceEndpointSG.connections.allowFrom(
        serviceSecurityGroup,
        Port.allTcp(),
      )
      
    httpsListener?.addTargets('ServiceTarget', {
      port: 80,
      targets: [service],
      healthCheck: {
        path: '/HealthCheck',
      },
    })
    httpListener?.addTargets('ServiceTarget', {
      port: 80,
      targets: [service],
      healthCheck: {
        path: '/HealthCheck',
      },
    })

    new MongoDBAtlasConstruct(
      this,
      'WorkflowConfiguratorMongoDBAtlasConsstruct',
      {
        roleArn: taskDefinition.taskRole.roleArn,
        serviceSG: service.connections.securityGroups[0] as SecurityGroup,
        mongoDB: {
          ...mongoDB,
          comment: `${tags.environment}-${tags.product}-${service.serviceName}`,
          roles: [
            {
              roleName: 'readWrite',
              databaseName: mongoDB.databaseName,
            },
          ],
        },
      },
    )
  }
}

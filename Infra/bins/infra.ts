#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { InfraStack } from '../lib/infra-stack';
import { WorkflowConfiguratorEcrStack } from '../lib/workflow-configurator-ecr-stack';
import { devConfig } from '../lib/configs/dev'
import { stagingConfig } from '../lib/configs/staging';
import { prodConfig } from '../lib/configs/efs-prod';
const app = new cdk.App();

const devECRStack = new WorkflowConfiguratorEcrStack(app, 'WorkflowConfiguratorEcrDevStack',
{
  env:{
    account: '030464729230',
    region: 'eu-central-1'
  }
})

new InfraStack(app, 'WorkflowConfiguratorDevStack',
  {
    ...devConfig(),
    repository: devECRStack.ecrRepository
    
})

const stagingECRStack = new WorkflowConfiguratorEcrStack(app, 'WorkflowConfiguratorEcrStagingStack',
{
  env:{
    account: '487727660817',
    region: 'eu-central-1'
  }
})
new InfraStack(app, 'WorkflowConfiguratorStagingStack',
    {
        ...stagingConfig(devECRStack.ecrRepository),
    repository: stagingECRStack.ecrRepository
    
})

const efsProdECRStack = new WorkflowConfiguratorEcrStack(app, 'WorkflowConfiguratorEFSEcrProdStack',
{
  env:{
    account: '457621392112',
    region: 'eu-central-1'
  }
})
new InfraStack(app, 'WorkflowConfiguratorEFSProdStack',
    {
        ...prodConfig(stagingECRStack.ecrRepository),
    repository: efsProdECRStack.ecrRepository,
    endpointSGParamName: 'VPC_INTERFACE_ENDPOINT_SEC_GROUP'
})

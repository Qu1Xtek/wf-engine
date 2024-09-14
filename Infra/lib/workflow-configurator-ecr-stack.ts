import { Repository } from 'aws-cdk-lib/aws-ecr';
import { OpenIdConnectProvider, PolicyStatement, Role, WebIdentityPrincipal } from 'aws-cdk-lib/aws-iam';
import { CfnOutput, Stack, StackProps } from 'aws-cdk-lib';
import { Construct } from 'constructs';
interface WorkflowConfiguratorEcrStackProps extends StackProps {
}

export class WorkflowConfiguratorEcrStack extends Stack {
public readonly ecrRepository: Repository
    constructor(scope: Construct, id: string, props?: WorkflowConfiguratorEcrStackProps) {
        super(scope, id, props);
        const repo = new Repository(this, 'WorkflowConfiguratorECR', {
            imageScanOnPush: true,

        });
        this.ecrRepository = repo

        const provider = OpenIdConnectProvider.fromOpenIdConnectProviderArn(
            this,
            'GitHubOidcProvideer',
            `arn:aws:iam::${this.account}:oidc-provider/token.actions.githubusercontent.com`);

        const ecrAccessRole = new Role(this, 'WorkflowConfiguratorEcrAccessRole', {
            assumedBy: new WebIdentityPrincipal(provider.openIdConnectProviderArn, {
                StringEquals: {
                    [`${provider.openIdConnectProviderIssuer}:sub`]:
                        'repo:Arxum/workflow-configurator:ref:refs/heads/main',
                },
            }),
        });
        repo.grant(
            ecrAccessRole,
            'ecr:PutImage',
            'ecr:InitiateLayerUpload',
            'ecr:UploadLayerPart',
            'ecr:CompleteLayerUpload',
            'ecr:BatchCheckLayerAvailability',
        );

        ecrAccessRole.addToPolicy(
            new PolicyStatement({
              actions: ['ecr:GetAuthorizationToken'],
              resources: ['*'],
            }),
          );
          new CfnOutput(this, `WorkflowConfiguratorECRName`, {
            value: repo.repositoryName,
          });
          new CfnOutput(this, `WorkflowConfiguratorAccessRole`, {
            value: ecrAccessRole.roleArn,
          });
          new CfnOutput(this, `WorkflowConfiguratorECRUri`, {
            value: repo.repositoryUri,
          });
    }
}
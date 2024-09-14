import { Secret } from 'aws-cdk-lib/aws-ecs'
import { StringParameter } from 'aws-cdk-lib/aws-ssm'
import { Construct } from 'constructs'

const getSecret = (scope: Construct, id: string, parameterName: string) => {
    return Secret.fromSsmParameter(
        StringParameter.fromSecureStringParameterAttributes(scope, id, {
            parameterName,
        }),
    )
}

export const secrets = (scope: Construct): { [key: string]: Secret } => ({
    DIP_PASSWORD: getSecret(scope, 'DIP_PASSWORD', 'DIP_ORG_PASSWORD'),
    USER_MGMT_TOKEN: getSecret(scope, 'USER_MGMT_TOKEN', 'USER_MGMT_TOKEN'),
    PRINTNODE_API_KEY: getSecret(scope, 'PRINTNODE_API_KEY', 'PRINTNODE_API_KEY'),
    BLAST_DEV_KC_SECRET: getSecret(scope, 'BLAST_DEV_KC_SECRET', 'BLAST_DEV_KC_SECRET'),
    KC_BLAST_DEV_ID: getSecret(scope, 'KC_BLAST_DEV_ID', 'KC_BLAST_DEV_ID'),
})

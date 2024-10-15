interface env {
    JwtSecret: string | undefined,
    GoogleClientId: string | undefined,
    GoogleClientSecret: string | undefined
}

export const env: env = {
    JwtSecret: process.env.JWT_SECRET,
    GoogleClientId: process.env.GOOGLE_CLIENT_ID,
    GoogleClientSecret: process.env.GOOGLE_CLIENT_SECRET
};
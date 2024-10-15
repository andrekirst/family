import NextAuth, { AuthOptions } from "next-auth";
import GoogleProvider, { GoogleProfile } from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials"
import { GoogleLogin, Login, LoginRequest } from "@/services/api/authentication"
import { env } from "@/app/lib/env";

const authOptions: AuthOptions = {
    providers: [
        GoogleProvider({
            clientId: env.GoogleClientId!,
            clientSecret: env.GoogleClientSecret!,
            authorization: {
                params: {
                    scope: "openid email profile"
                }
            }
        }),
        CredentialsProvider({
            name: "Credentials",
            credentials: {
                login: { label: "Login", type: "text", placeholder: "test", value: "test" },
                password: { label: "Password", type: "password", value: "test" }
            },
            async authorize(credentials, req) {
                try {
                    var request: LoginRequest = {
                        login: credentials?.login!,
                        password: credentials?.password!,
                    };

                    var response = await Login(request);

                    if (response) {
                        return {
                            id: response.Id,
                            email: response.email,
                            name: response.username,
                            token: response.token
                        };
                    }
                } catch (error) {
                    console.error("Login failed", error);
                    return null;
                }

                return null;
            }
        })
    ],
    callbacks: {
        async signIn({ account, profile, credentials, user }) {
            if (account?.provider === 'google') {
                try {
                    var googleProfile = profile as GoogleProfile;
                    const response = await GoogleLogin({
                        email: profile?.email!,
                        name: profile?.name!,
                        googleId: profile?.sub!,
                        googleAccessToken: account?.id_token,
                        accessToken: account?.access_token,
                        lastName: googleProfile.family_name,
                        firstName: googleProfile.given_name
                    });

                    if(response)
                    {
                        account.access_token = response.token;
                        return true;
                    }
                    
                } catch (error) {
                    console.log(error);
                }
            }

            if (account?.provider === 'credentials') {
                const response = await Login({
                    login: credentials?.login.value!,
                    password: credentials?.password.value!
                })

                if (response) {
                    return true;
                }
            }

            return false;
        },
        async jwt({ token, account, user, session }) {
            if (account && user) {
                token.id = user.id;

                if(account.provider === 'google') {
                    token.accessToken = account.access_token;
                }
                else {
                    token.accessToken = user.token;
                }
            }

            return token;
        },
        async session({ session, token, newSession }) {
            // @ts-ignore
            session.id = token.id;
            // @ts-ignore
            session.accessToken = token.accessToken;
            // @ts-ignore
            session.idToken = token.idToken;
            // @ts-ignore
            session.user.id = token.id;
            // @ts-ignore
            session.user.accessToken = token.accessToken;
            return session;
        }
    },
    session: {
        strategy: "jwt",
        maxAge: 2592000 // 30 days
    },
    secret: env.JwtSecret
};

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
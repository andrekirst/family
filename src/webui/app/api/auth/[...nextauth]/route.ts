import NextAuth, { AuthOptions } from "next-auth";
import GoogleProvider, { GoogleProfile } from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials"
import { GoogleLogin, Login, LoginRequest } from "@/services/api/authentication"

const authOptions: AuthOptions = {
    providers: [
        GoogleProvider({
            clientId: process.env.GOOGLE_CLIENT_ID!,
            clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
            authorization: {
                params: {
                    scope: "openid email profile"
                }
            }
        }),
        CredentialsProvider({
            name: "Credentials",
            credentials: {
                username: { label: "Username", type: "text", placeholder: "test", value: "test" },
                email: { label: "EMail", type: "text", placeholder: "test@test.de", value: "test@test.de" },
                password: { label: "Password", type: "password", value: "test" }
            },
            async authorize(credentials, req) {
                try{
                    var request: LoginRequest = {
                        login: credentials?.username!,
                        email: credentials?.email!,
                        password: credentials?.password!,
                    };

                    var response = await Login(request);

                    if(response) {
                        return {
                            id: response.Id,
                            email: response.email,
                            name: response.username
                        };
                    }
                } catch(error) {
                    console.error("Login failed", error);
                    return null;
                }

                return null;
              
            }
        })
    ],
    callbacks: {
        async signIn({ account, profile, user }) {
            if(account?.provider === 'google') {
                try {
                    var googleProfile = profile as GoogleProfile;
                    const response = await GoogleLogin({
                        email: profile?.email!,
                        name: profile?.name!,
                        googleId: profile?.sub!,
                        accessToken: account?.id_token,
                        lastName: googleProfile.family_name,
                        firstName: googleProfile.given_name });
                    return response;
                } catch (error) {
                    console.log(error);
                }

                return false;
            }

            return true;
        },
        async jwt({ token, account, user, session, profile }) {
            if (user) {
                token.id = user.id;
            }

            if(account) {
                token.access_token = account.access_token;
                token.idToken = account.id_token;
            }

            return token;
        },
        async session({ session, token }) {
            session.id = token.id;
            session.accessToken = token.accessToken;
            session.idToken = token.idToken;
            return session;
        }
    },
    session: {
        strategy: "jwt",
        maxAge: 2592000 // 30 days
    }
};

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
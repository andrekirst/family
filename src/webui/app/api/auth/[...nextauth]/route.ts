import NextAuth, { AuthOptions } from "next-auth";
import GoogleProvider, { GoogleProfile } from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials"
import { GoogleLogin, Login, LoginRequest } from "@/services/api/authentication"

export const authOptions: AuthOptions = {
    providers: [
        GoogleProvider({
            clientId: process.env.GOOGLE_CLIENT_ID!,
            clientSecret: process.env.GOOGLE_CLIENT_SECRET!
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
        async signIn({ account, profile }) {
            if(account?.provider === 'google') {
                try {
                    var googleProfile = profile as GoogleProfile;
                    const response = await GoogleLogin({
                        email: profile?.email!,
                        name: profile?.name!,
                        googleId: profile?.sub!,
                        accessToken: account?.access_token,
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
        async jwt({ token, user }) {
            if (user) {
                token.id = user.id;
            }

            return token;
        },
        async session({ session, token }) {
            session.id = token.id;
            return session;
        }
    },
    session: {
        strategy: "jwt",
        maxAge: 2592000 // 30 days
    }
};

export const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
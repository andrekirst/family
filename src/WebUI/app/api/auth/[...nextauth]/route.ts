import NextAuth, { AuthOptions } from "next-auth";
import GoogleProvider from "next-auth/providers/google";

export const authOptions: AuthOptions = {
    providers: [
        GoogleProvider({
            clientId: process.env.GOOGLE_CLIENT_ID!,
            clientSecret: process.env.GOOGLE_CLIENT_SECRET!
        })
    ],
    callbacks: {
        async jwt({ token, user }) {
            if(user) {
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
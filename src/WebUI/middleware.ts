import { NextResponse } from 'next/server';
import { getToken } from 'next-auth/jwt';
import { NextRequest } from 'next/server';

export async function middleware(req: NextRequest) {
    const { pathname } = req.nextUrl;

    // Vermeide eine Endlosschleife, indem du Anfragen an `/api/auth` ignorierst
    if (pathname.startsWith('/api/auth')) {
        return NextResponse.next();
    }

    // Überprüfen, ob der Benutzer authentifiziert ist
    const token = await getToken({ req });

    // Wenn kein Token vorhanden ist, leite den Benutzer zur NextAuth-Anmeldeseite weiter
    if (!token) {
        const signInUrl = new URL('/api/auth/signin', req.url); // Standard-NextAuth-Loginseite
        return NextResponse.redirect(signInUrl);
    }

    // Setze die Anfrage fort, wenn der Benutzer authentifiziert ist
    return NextResponse.next();
}

// Routen, auf die die Middleware angewendet wird
export const config = {
    matcher: ['/:path*'], // Geschützte Routen angeben
};
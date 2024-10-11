import { NextRequest, NextResponse } from "next/server";

export default function middleware(request: NextRequest) {    
    const pathname = request.nextUrl.pathname;
    const token = request.cookies.get('next-auth.session-token');

    if(pathname === '/' && !token) {
        const url = request.nextUrl.clone();
        url.pathname = '/auth/unauthorized';
        return NextResponse.redirect(url);
    }

    return NextResponse.next();
}

export const config = {
    matcher: [
      // match all routes except static files and APIs
      "/((?!api|_next/static|_next/image|favicon.ico).*)",
    ],
  };
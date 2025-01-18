# DayBuddy

DayBuddy is a full-stack dating platform designed to provide a unique, interest-based matching experience for users. The platform is built using modern web technologies and offers real-time messaging, notifications, and a range of features for both users and administrators.

![DayBuddy Overview](DayBuddy/wwwroot/Assets/DayBuddyOverview.gif)

## Features

### User Features
- **Profile Management**:
  - Edit profile information, including name, age, interests, sexuality, and gender.
  - Secure authentication with email verification using ASP.NET Identity.
- **Matching System**:
  - Get matched every 8 hours with a random person who shares similar interests.
  - Option to report or block users.
  - Unmatching requires waiting for the next 8-hour cycle (except for premium users).
  - No "likes" feature—users are automatically matched based on shared interests.
- **Premium Accounts**:
  - No cooldown period for matching.
  - Ad-free experience.
- **Real-Time Communication**:
  - Chat with matches using SignalR-powered real-time messaging and notifications.

### Admin Features
- **Admin Panel**:
  - Manage user feedback.
  - Grant premium subscriptions to users.

### Monetization
- Integrated with Stripe for account purchasing.
- Google AdSense ads for non-premium users.

## Technology Stack

### Frontend
- Razor Pages
- HTML, CSS, JavaScript
- Bootstrap for responsive UI design
- jQuery for dynamic interactivity

### Backend
- ASP.NET Core with SignalR for real-time features

### Database
- MongoDB for storing user profiles, matches, feedback, and reports

### Payment and Ads
- Stripe for premium account purchasing
- Google AdSense for advertisements

## Usage

### User Workflow
1. Sign up and verify your email.
2. Complete your profile by adding details such as age, interests, and gender.
3. Wait for the 8-hour match cycle (or upgrade to premium for instant matching).
4. Chat with your match through the real-time messaging system.
5. Optionally block or report users if necessary.

### Admin Workflow
1. Use MongoDb Compas to edit the user and add the Admin Role
1. Open the admin panel.
2. View and manage user feedback.
3. Grant premium accounts to users as needed.

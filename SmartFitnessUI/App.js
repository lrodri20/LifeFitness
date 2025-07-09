// App.js
import React, { useContext } from 'react';
import { TouchableOpacity } from 'react-native';
import { NavigationContainer, getFocusedRouteNameFromRoute } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { navigationRef } from './navigation/NavigationService';
import { Ionicons } from '@expo/vector-icons';

import { AuthProvider, AuthContext } from './context/AuthContext';
import SignInScreen from './screens/SignInScreen';
import SignUpScreen from './screens/SignUpScreen';
import ForgotPasswordScreen from './screens/ForgotPasswordScreen';
import HomeScreen from './screens/HomeScreen';   // Activities screen
import MatchesScreen from './screens/MatchesScreen';
import LikesScreen from './screens/LikesScreen';
import MessagesScreen from './screens/MessagesScreen';
import SettingsScreen from './screens/SettingsScreen';
import ViewProfileScreen from './screens/ViewProfileScreen';
const AuthStack = createStackNavigator();
const AppStack = createStackNavigator();
const Tab = createBottomTabNavigator();
function getHeaderTitle(route) {
  const routeName = getFocusedRouteNameFromRoute(route) ?? 'Matches';
  switch (routeName) {
    case 'Matches':
      return 'Matches';
    case 'Likes':
      return 'Likes';
    case 'Activities':
      return 'Activities';
    case 'Messages':
      return 'Messages';
    default:
      return 'Home';
  }
}
function MainTabs() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarShowLabel: false,
        tabBarStyle: {
          backgroundColor: '#fff',
          borderTopWidth: 0,
          elevation: 5,
          height: 60,
        },
        tabBarIcon: ({ focused, color, size }) => {
          let iconName;
          switch (route.name) {
            case 'Matches':
              iconName = focused ? 'people' : 'people-outline';
              break;
            case 'Likes':
              iconName = focused ? 'heart' : 'heart-outline';
              break;
            case 'Activities':
              iconName = focused ? 'calendar' : 'calendar-outline';
              break;
            case 'Messages':
              iconName = focused ? 'chatbubble' : 'chatbubble-outline';
              break;
            default:
              iconName = 'ellipse';
          }
          return <Ionicons name={iconName} size={24} color={focused ? '#4CAF50' : '#888'} />;
        },
      })}
    >
      <Tab.Screen name="Matches" component={MatchesScreen} />
      <Tab.Screen name="Likes" component={LikesScreen} />
      <Tab.Screen name="Activities" component={HomeScreen} />
      <Tab.Screen name="Messages" component={MessagesScreen} />

    </Tab.Navigator>
  );
}

function RootNavigator() {
  const { userToken, isLoading } = useContext(AuthContext);

  if (isLoading) {
    // Could show splash
    return null;
  }

  return (
    <NavigationContainer ref={navigationRef}>
      {userToken == null ? (
        <AuthStack.Navigator screenOptions={{ headerShown: false }}>
          <AuthStack.Screen name="SignIn" component={SignInScreen} />
          <AuthStack.Screen name="SignUp" component={SignUpScreen} />
          <AuthStack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
        </AuthStack.Navigator>
      ) : (
        <AppStack.Navigator>
          <AppStack.Screen
            name="Main"
            component={MainTabs}
            options={({ route, navigation }) => ({
              title: getHeaderTitle(route),
              headerTitleAlign: 'center',
              headerRight: () => (
                <TouchableOpacity onPress={() => navigation.navigate('Settings')} style={{ marginRight: 16 }}>
                  <Ionicons name="settings-outline" size={24} color="#000" />
                </TouchableOpacity>
              ),
              headerTintColor: '#4CAF50',
            })}
          />
          <AppStack.Screen
            name="ViewProfile"
            component={ViewProfileScreen}
            options={{
              title: 'User Profile',
              headerTitleAlign: 'center',
              headerTintColor: '#4CAF50',
            }}
          />
          <AppStack.Screen
            name="Settings"
            component={SettingsScreen}
            options={{
              title: 'Profile Settings',
              headerTitleAlign: 'center',
              headerTintColor: '#4CAF50',
            }}
          />
        </AppStack.Navigator>
      )}
    </NavigationContainer>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <RootNavigator />
    </AuthProvider>
  );
}
